/*
 * Copyright 2002-2026 the original author or authors.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Spring.IntegrationTests;

/// <summary>
/// Provisions the ActiveMQ broker that the messaging integration test assemblies run against, and publishes where it is.
/// </summary>
/// <remarks>
/// <para>
/// This file is compiled into each ActiveMQ backed integration test project (a linked source file, not a project of its
/// own), and the assembly's <c>[SetUpFixture]</c> calls <see cref="InitializeAsync"/> and <see cref="DisposeAsync"/>. Every
/// assembly therefore gets its own broker, on a random host port, so the assemblies can run in parallel with the SQL Server
/// backed ones (see <c>SqlServerTestEnvironment</c>) without port clashes.
/// </para>
/// <para>
/// By default an ActiveMQ Classic container is started with Testcontainers, which needs only a running Docker daemon. When
/// the environment variable <see cref="BrokerUriVariable"/> is already set, to the URI of a broker, no container is started
/// and the tests use that broker instead (for a developer without a Linux container engine).
/// </para>
/// <para>
/// Either way, the tests learn where the broker is from <see cref="BrokerUriVariable"/>. The Spring XML contexts read it
/// through a <c>${SPRING_TEST_ACTIVEMQ_BROKER_URI}</c> placeholder; C# code uses <see cref="GetBrokerUri"/>.
/// </para>
/// </remarks>
internal static class ActiveMqTestEnvironment
{
    /// <summary>
    /// The image the container runs: ActiveMQ Classic, whose OpenWire transport is what <c>Apache.NMS.ActiveMQ</c> speaks.
    /// Pinned, so that a run does not change with the registry's "latest".
    /// </summary>
    public const string Image = "apache/activemq-classic:6.2.0";

    /// <summary>
    /// The environment variable that holds the broker's URI (<c>tcp://host:port</c>). If it is set before
    /// <see cref="InitializeAsync"/> runs, that broker is used and no container is started.
    /// </summary>
    public const string BrokerUriVariable = "SPRING_TEST_ACTIVEMQ_BROKER_URI";

    /// <summary>
    /// The port of the OpenWire transport inside the container.
    /// </summary>
    public const int OpenWirePort = 61616;

    private static readonly SemaphoreSlim s_lock = new(1, 1);
    private static IContainer s_container;
    private static bool s_initialized;

    /// <summary>
    /// Starts the broker, unless <see cref="BrokerUriVariable"/> already names one, and publishes
    /// <see cref="BrokerUriVariable"/>.
    /// </summary>
    public static async Task InitializeAsync()
    {
        await s_lock.WaitAsync();
        try
        {
            if (s_initialized)
            {
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(BrokerUriVariable)))
                {
                    s_container = new ContainerBuilder(Image)
                        .WithPortBinding(OpenWirePort, true)
                        // The broker logs this line once the OpenWire transport accepts connections. The mapped port
                        // accepts a connection earlier than that, because Docker's proxy listens on behalf of the container.
                        .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged("Connector openwire started"))
                        .Build();
                    await s_container.StartAsync();

                    Environment.SetEnvironmentVariable(
                        BrokerUriVariable,
                        $"tcp://{s_container.Hostname}:{s_container.GetMappedPublicPort(OpenWirePort)}");
                }

                s_initialized = true;
            }
            catch
            {
                // Report the failure of the setup, not one of the cleanup after it.
                try
                {
                    await DisposeContainerAsync();
                }
                catch (Exception)
                {
                    // The container's Ryuk reaper removes it anyway.
                }

                throw;
            }
        }
        finally
        {
            s_lock.Release();
        }
    }

    /// <summary>
    /// Stops the container, if one was started.
    /// </summary>
    public static async Task DisposeAsync()
    {
        await s_lock.WaitAsync();
        try
        {
            s_initialized = false;
            await DisposeContainerAsync();
        }
        finally
        {
            s_lock.Release();
        }
    }

    /// <summary>
    /// Gets the broker's URI, <c>tcp://host:port</c>.
    /// </summary>
    public static string GetBrokerUri()
    {
        return Environment.GetEnvironmentVariable(BrokerUriVariable)
               ?? throw new InvalidOperationException(
                   $"'{BrokerUriVariable}' is not set. Run the test from an assembly whose [SetUpFixture] calls ActiveMqTestEnvironment.InitializeAsync.");
    }

    private static async Task DisposeContainerAsync()
    {
        IContainer container = s_container;
        s_container = null;
        if (container is not null)
        {
            // The variable names this container's broker; do not leave it pointing at a broker that is gone.
            Environment.SetEnvironmentVariable(BrokerUriVariable, null);
            await container.DisposeAsync();
        }
    }
}
