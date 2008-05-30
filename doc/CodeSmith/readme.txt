CodeSmith

CodeSmith allows you to create templates that will generate code for any ASCII based language.  The code generated can be customized by the use of properties.  A property can be any .NET object that has a designer (most built in .NET types have designers already) and can be as simple as a boolean property that allows you to conditionally add or remove code from the result, to an object such as the included TableSchema object which provides everything you could possibly want to know about a table in a database.  Having access to this information allows you to generate things such as stored procedures, business objects, presentation layer code, or anything else you can think of based on a table schema.

CodeSmith's syntax is almost identical to ASP.NET. So if you are familiar with ASP.NET then you should be able to quickly learn the template syntax.  You can use the C#, VB.NET or JScript.NET languages in your templates.

The CodeSmith Explorer window allows you to view all templates in a given folder and allows you to drag and drop to any target that supports dropping text.  You can also double-click a template to execute it individually after the first time you run CodeSmith.

I hope to receive feedback, bug reports and hopefully some useful templates to include in future versions.  I am also interested if someone would be willing to help with documentation. CodeSmith is a FREEWARE product. I am still considering releasing source code, but I would rather maintain control of the product in a single location. I think this will provide the best opportunity to build a solid community of users and templates. I also plan to build a template repository site that will allow anyone to submit a template and have it added to the repository.

Enjoy,
Eric J. Smith
http://www.ericjsmith.net/codesmith/

--------------------------------------------------
History
--------------------------------------------------
Build 2.6.0 (RC1)
  * Added GetCodeTemplateInstance method to CodeTemplate.  This can be used to compile and create an instance of another template.
  * Added Toggle Template Code Expansion feature.  This allows easy viewing of the static content in the template. (Shortcut CTRL-SHIFT-M)
  * Fixed up the syntax highlighting editor dialog and made it persist the settings.
  * Made it so that you can manually enter a delimited list of strings for StringCollection in the property grid.
  * Added several options to the options dialog.
  * Improved outlining.
  * Fixed various minor bugs.
Build 2.5.18 (BETA)
  * More performance improvements in the core CodeSmith engine.
  * Made it so that if you reselect the same schema object it will refresh the schema information.
  * Made the enter key open the selected template in CodeSmith Studio.
  * Fixed bug with saving a property set file while overwriting an existing property set file that is set to read only.
  * Made CodeSmith Studio a single instance application.
  * Fixed bug where trying to open a file that was already open would cause the file to re-compile itself.
  * Fixed bug when compiling templates that have ( ) or ' in their file names.
Build 2.5.17 (BETA)
  * Fixed issue with CodeSmith Studio hanging sometimes when <% was typed.
  * Made the VS.NET custom tool MUCH better.  It now reports errors in much more detail.
  * Added variable support to the VS.NET custom tool.
  * Added default property support to the VS.NET custom tool.
  * Made it so that goto line now expands the regions the line is on.
Build 2.5.16 (BETA):
  * Made various performance improvements to the engine and CodeSmith Studio.
  * Added syntax highlighting to target languages.
  * Added outlining support to CodeSmith Studio.
  * Added line modification markers to CodeSmith Studio.
  * Added auto copy output to clipboard option in CodeSmith Studio.
  * Fixed several find and replace bugs.
Build 2.5.15 (BETA):
  * Added ToString override to the schema collections so that the names of the selected objects show up.
  * Stopped indenting script blocks.
  * Fixed highlighting issues with escaped template tags <%% %%>.
  * Made register menu item hidden when already registered.
  * Fixed bug where pressing F4 on codebehind causes exception.
  * Changed RenderToFile using a merge strategy so that it creates a file if it doesn't exist.
  * Changed the output encoding to UTF-8.
  * Fixed bug in editor control where a black box was sometimes drawn.
  * Fixed bug in editor control where a clipboard operation would sometimes cause an exception.
  * Updated to the 1.5 version of Chris Nahr's collection templates.
  * Various other minor bug fixes.
Build 2.5.14 (Final):
  * Turned CodeSmith Professional licensing on.
Build 2.5.13 (RC4):
  * Fixed the check to see if a given file is already open (was case-sensative).
  * Fixed issue with setting properties programmatically if they were not an exact type match but were still related types.
  * Disabled the replace and replace all buttons on the find dialog if a document is read only.
  * Fixed the StoredProcedures.cst template to handle user defined types.
  * Made it so that the explorer tree doesn't do a complete refresh on every file save.
  * Fixed painting issues in the Highlighting Style Editor dialog.
  * Fixed issue with the find function not always moving the find result into view.
  * Fixed issue with CTRL-TAB and new documents.
  * Fixed template parser to allow escaped "'s in the directive attributes.
Build 2.5.12 (RC3):
  * Fixed bug in SqlSchemaProvider where tables with .'s in their name would cause an error.
  * Changed SchemaExplorer to lazy load extended properties.  This made a huge difference in databases with a lot of schema objects. 
  * Fixed bug with RenderToFile where the file handle was not being released if an error occured during template execution.
  * Added the awesome DBDocumenter templates to the samples.
  * Added the C# CSLA.NET templates by Ricky Supit.
  * Added the StoredProcedureDescriptions.cst template by Oskar Austegard.
  * Fixed bug with save all button where not all documents would be saved.
  * Added context menu to the output and compiled source editors.
  * Fixed bug in the logo header of CodeSmithConsole.
  * Fixed bug with determining if a file has been modified in CodeSmith Studio.
  * Fixed various issues with the goto line feature in CodeSmith Studio.
  * Fixed formatting issue with template comment tags.
  * Fixed bugs with commands enabling and disabling in CodeSmith Studio.
  * Changed the F6 mapping in Studio to toggle between views of the current document.
  * Changed build shortcut to CTRL-SHIFT-B.
  * Fixed bug with external change modification notice.  Whenever you closed a document and re-opened it you would then get errant external change modification notices.
  * Fixed bug with CTRL-F sometimes causing a crash.
  * Fixed various painting issues in the editor control.
  * Added option to determine code behind open behaviour in CodeSmith Studio.
  * Fixed issue with various menu item actions not updating the document title.
  * Changed CTRL-TAB and CTRL-SHIFT-TAB to behave the same as VS.NET.
  * Added ability to select template editor application from CodeSmith Explorer.
  * Made it so the template will recompile if the code behind file has been modified.
Build 2.5.11 (RC2):
  * Fixed parser bug where whitespace would not be correctly handled in some scenerios.
  * Fixed bug with custom assembly resolution.
  * Fixed bug when closing multiple instances of Studio at the same time.
  * Fixed bugs in a few sample templates.
  * Fixed bug when using a \ in the find dialog.
Build 2.5.10 (RC1):
  * Added some new help content.  Thanks to James Avery.
  * Updated to the latest collection and CSLA.NET templates.
  * Added State (values: Default, Rendering, Validating, RestoringProperties, SavingProperties) property to CodeTemplate.  This can be used to tell what the template is currently doing.
  * Fixed bug when saving a newly created template.
  * Made the close start page on open setting work for all ways of opening files.
  * Fixed bug in collections where indexers threw an exception for items that did not exist.  These indexers now return null if the item is not found.
  * Added override for ToString() in CodeSmith.CustomProperties.StringCollection so that the items show up in the propertygrid instead of the type name.
  * Fixed bug in RestorePropertiesFromHashtable where you get a NullReferenceException when trying to populate a property that has been removed since the last compile.
  * Fixed bug in the about box where some names were being cut off.
  * Fixed bug with Load Property Set XML in the stand-alone property grid.
  * Fixed bug in CodeSmith Explorer with template folders that no longer exist.
  * Fixed bug in CodeSmith Studio with opening files that no longer exist.
Build 2.5.9 (Beta):
  * Added context menus to the document tabs.
  * Added CopyPropertiesTo method to CodeTemplate.  This can be used to copy all matching properties from one template instance to another.
  * Dramatically improved compiler performance on large templates.
  * Lots of improvements to the CodeSmith Explorer control.
  * Rebuilt all of the Schema Explorer collections using the awesome collection templates by Chris Nahr.  These collections are now editable, although the instances returned by Schema Explorer are marked as read-only.
  * Fixed bug where enum values were not being maintained during template compilation.
  * Fixed bug where new files were not added to the MRU list.
  * All configuration files have now been moved to the current user's ApplicationData folder.  It should now be possible to run CodeSmith as a non-Administrator user.
  * Added folder monitoring to Explorer so that new files are automatically picked up.
  * Added monitoring to all documents in CodeSmith Studio so that external changes are picked up.
Build 2.2.8 (Beta):
  * Added tool tip to document tabs in CodeSmith Studio.
  * Added keyboard shortcuts to almost everything in CodeSmith Studio.
  * Added ability to open any file type in CodeSmith Studio.
  * Added error underlines to the compiled template source when there are compilation errors.  These also have tooltips to display the error message.
  * Added Select All, Copy Output, Save Output, and Compile To Assembly menu items.
  * Added Insert Content menu items and shortcuts.
  * Added dialog to ask if you want to open the code behind for a template if it uses one.  (this really should be another tab in the template editor.)
  * Added F3 support and made various fixes to the find and replace operations.
  * Added menu item to save output to file.
  * Added Windows menu to CodeSmith Studio.
  * Added recent files menu to CodeSmith Studio.
  * Added context sensative enabling/disabling of commands in CodeSmith Studio.
  * Added options dialog with various settings.
  * Fixed parser bug where comments (<%-- --%>) would collapse a line with other content on it.
  * Added ability to change language background color in the highlighting style editor dialog.
  * Added start page / embedded web browser to CodeSmith Studio.
Build 2.2.7 (Beta):
  * The Schema Explorer tool window in CodeSmith Studio works now (still need to have it let you manage your extended properties).
  * The properties grid is now used to show properties on just about everything.
  * Fixed bug when dropping a template onto the VS.NET Solution Explorer window.
  * Updated to the latest version of the CSLA.NET templates.
  * Created a new sample template CommandWrapperClass.cst.  This template creates a C# wrapper class for a stored procedure.
  * There is now a Description property on all schema objects.  This is simply a shortcut to the CS_Description extended property.
  * Made the find dialog set the currently selected text as the find value instead of it being hard-coded to "int".
  * Made the find dialog restore focus to the editor window when the dialog is closed.
  * Fixed bug in Studio where the same instance of a template was being used for multiple generations.
  * Added CS_IsComputed and CS_IsDeterministic extended properties to ColumnSchema and ViewColumnSchema.
  * Added CS_Default extended property to ParameterSchema.
  * Added CTRL-TAB and CTRL-SHIFT-TAB support to CodeSmith Studio.
  * Fixed printing issue in CodeSmith Studio where lines were not being wrapped.
  * Fixed bug in parser that caused <% = expression %> (space between <% and =) to be incorrectly parsed.
  * Fixed bug in parser that caused multi-line template comments to be incorrectly parsed.
  * Updated to version 1.3.1 of Chris Nahr's collection templates.
  * Created CodeSmith101 sample templates.
Build 2.2.6 (Beta):
  * Implemented cursor changes in CodeSmith Studio when the application is working.
  * Fixed bug in extended properties where extended property value was NULL.
  * Fixed several clipboard issues in CodeSmith Studio.
  * Added error trapping around template execution so that it's obvious the exception was from bad code in the template and not CodeSmith.
  * Updated to version 1.3.0 of Chris Nahr's collection templates.
  * Fixed bug in CodeSmith Studio where template properties would be lost after a failed compilation.
  * Added code to allow enum property types defined in multiple templates to be converted back and forth.
Build 2.2.5 (Beta):
  * Added icons to the Visual Studio .NET tool window and command.
  * Added a blank data source to all designers.
  * Added context menu items to the property grid to copy, save and load the property set XML.
  * Fixed bug with right-clicking in the property grid on a category cell.
  * Made a lot of internal changes to cleanup the way CodeSmith was searching for assemblies.
  * Replaced VSUserControlHost with CodeSmithUserControlHost.  I believe this change will fix the infamous "Invalid VSUserControlHost" error message in Visual Studio.
  * Added a ScriptTableData.cst sample template.
  * Made CodeSmith Studio persist highlighting style changes.
  * Added Schema Explorer tool window to CodeSmith Studio.
  * Fixed highlighting bug where single line comments would over-ride the end of a template block (%>).
  * Added highlighting support for template comments (<%-- %>).
  * Implemented alternate method of retrieving command schema result information in certain scenerios where it would fail otherwise.
  * Fixed bug with SQL7 and views.
  * Added option to installer to select whether or not VS.NET support is installed.
  * Made it so that Undo buffer is cleared right after document load.
Build 2.2.4 (Beta):
  * Added GetProperties and GetRequiredProperties to CodeTemplate.
  * Added AllInputParameters, AllOutputParameters, and NonReturnValueParameters to the CommandSchema object.
  * Fixed various exceptions in CodeSmith Studio.
  * Added EngineSample (this was previously ConsoleSample).
  * Added ConsoleSamples.  This contains various samples of using the command line client.
  * Added option to installer to include Visual Studio .NET 2003 support.
  * Made CodeSmith Studio handle saving to read-only files.
  * Fixed issue with command parameters extended properties.
  * Added new sample template that outputs all extended properties for a database.
Build 2.2.3 (Beta):
  * Fixed parser bug where first literal line of template would be parsed incorrectly and discarded.
  * Fixed parser bug where <script> tags were being incorrectly parsed.
  * Fixed bug in Property directive Default attribute where defaults would only work with properly cased types.
  * Added ParseDefaultValue to CodeTemplate.  This method is called to parse the Property directive Default attribute value and assign the default value.  The method is virtual and can be overridden to extend the supported data types.
  * The Property directive Default attribute now supports enum values as well as all integer data types (was Int32 only before).
  * Made it so that you can press delete on a folder in the CodeSmith Explorer control to remove a folder.  Also, made it so that the full folder path is shown when hovering over the folder.
Build 2.2.2 (Beta):
  * Added merge capabilities (let's you merge into a region of an existing file) to CodeSmithConsole.
  * Redesigned CodeSmithConsole parameters and switches.
  * Added owner support to collections (indexer, Contains, IndexOf).
  * Fixed issue with property sets not containing the owner information.
  * Fixed bug where a SQL float type would be mapped to a Decimal instead of a Double.
Build 2.2.1 (Beta):
  * Added CodeSmith Studio.
  * Added CommandSchema.CommandResults to allow for discovery of command results schema information.
  * Added ViewSchema.ViewResult to allow for discovery of view result schema information.
  * Added StringCollection to the CodeSmith.CustomProperties assembly.  This class fully supports the CodeSmith GUI and XML property set.
  * Added XmlSerializedTypeConvertor to the CodeSmith.CustomProperties assembly.  This class allows conversion of XML serialized objects back and forth to the property set format.
  * Added ability to save property set XML to file in the generator form.
  * Added ability to load property set from XML file in the generator form.
  * Added SavePropertiesToXmlFile and RestorePropertiesFromXmlFile to CodeTemplate.
  * Added support for executing a template in CodeSmithConsole.
  * Added more error handling support to CodeSmithConsole.
  * Fixed bug with TableSchema extended properties.
  * Made database extended properties populate.
  * Fixed bug with version check.
  * Added a custom exception dialog that can be expanded to show the stack trace in SchemaExplorer.
  * Fixed problem with SchemaExplorer and non-dbo owners.  Also added owner information into the designers.
  * Changed designers to default to the recently modified or added data source from the data source manager.
  * Updated to version 1.2.1 of Chris Nahr's collection templates.
  * Included CSLA.NET templates from Matt Altadonna.
  * Made all fields required on the data source dialog.
  * Modified XmlSerializedFilePicker to not require an attribute named type on the root element.  It will now assume the type of the property.
  * Made parser ignore duplicate import and assembly directives.
  * Included some nice synchronization changes to the CSVector.cst template contributed by Joel Mueller.
  * Changed CommandSchema.InputParameters to only return Input parameters.  Previously it returned Input and InputOutput parameters.
  * Changed CommandSchema.OutputParameters to only return Output parameters.  Previously it returned Output and InputOutput parameters.
  * Added CommandSchema.InputOutputParameters to return InputOutput parameters for the command.
  * Added CommandSchema.ReturnValueParameter to provide a reference to the return value parameter of a command.
  * Fixed bug with CommandSchema and copy property set XML.
Build 2.1.1270 (Release):
  * Fixed bug with copy property set XML and null values.
  * Added context menu to the property grid that allows clearing values and turning the help panel on and off.
  * Added ViewText to the ViewSchema object.
  * Implemented a workaround for the VB.NET on .NET 1.0 issue.
  * Added designers for TableSchemaCollection, CommandSchemaCollection, and ViewSchemaCollection.  These types can now be used as properties in the GUI and in the VS.NET custom tool.
  * Changed all Schema Explorer collections to make use of the new value based Equals method when calling Contains and IndexOf.
  * Added ability to select multiple root folders in CodeSmith Explorer.
  * Enabled the maximize button on the explorer and generator forms.
  * Included the awesome C# collection templates built by Chris Nahr.  Hopefully someone will be willing to convert these to VB.NET and they can be included too.
  * Implemented the Equals and GetHashCode methods for each schema object.  Equals will now check for value equality instead of reference equality.
  * Made all schema explorer designers re-sizable and also made them bigger by default.
  * Fixed various bugs in the generator form.
  * Added CTRL-G to the generator form as a shortcut to the Generate button.
  * Added CTRL-T to the generator form as a shortcut to the copy template output to clipboard button.
  * Made it so that if you cancel out of a schema explorer designer, it will not null out your current selection.
  * Made it so that if you had a previous selection the schema explorer designers will automatically select that item.
  * Fixed a bug in the GUI where it would use the same instance of the template for each generation.
  * Added CustomProperties sample.  This sample demonstrates using the XmlSerializer to deserialize an object and use it as a property.
  * Added ExtendedProperties collection to all schema objects.  These can be used to hold any custom data.
  * Made SqlSchemaProvider populate all extended properties from SQL 2000 into the ExtendedProperties collection on each schema object.
  * Made SqlSchemaProvider populate five additional extended properties on each column: CS_IsIdentity, CS_IdentitySeed, CS_IdentityIncrement, CS_IsRowGuidCol, and CS_Default
  * Added CommandText property to the CommandSchema object.  This can be used to get the code for the command.
  * Added IsDependantOf method to the TableSchema object.  This will determine if a given table is a dependant of another table by crawling the ForeignKey heirarchy.
  * Added TableDependancyComparer class.  This can be used to sort tables in order of dependancy.
  * Made all Schema Explorer designers remember their last selected data source.
  * Changed Indexes collection to include the primary key index.  I was explicitly excluding this index before.
  * Added IsUnique to ColumnSchema.  This simply checks for a unique index on this column.
  * Added IsPrimaryKey, IsUnique, and IsClustered properties to IndexSchema.
  * Added DateCreated to TableSchema, ViewSchema, and CommandSchema.
  * Added support for Debug attribute in the CodeTemplate directive.  This must be set to true before you will be able to debug your templates.
  * Fixed various bugs in the SqlSchemaProvider assembly.
  * Added Ctrl-A support to the output panels.
  * Added Ctrl-G (go to line) support on the Compiled Template Source panel.
  * Added error handling to CodeSmithConsole and changed it to return a non-zero number if it fails.
  * Added System.Data and System.Drawing as default assembly references.
  * Added System.Data as a default namespace import.
  * Fixed a couple of bugs in the StoredProcedures.cst and added a AllStoredProcedures.cst.  This new template will generate stored procedures for every table in the database.
  * Added typed DataSet sample template and test application (TypedDataSetTester).  These are a work in progress but I am hoping to get feedback.
  * Fixed bug with SQL7 compatability.
  * Made further changes to the installer so that it will hopefully work for more people.
  * Re-fixed a bug with command parameters.  This accidentally got reverted.
Build 2.0.1245:
  * Added SampleCustomProperties project.  This project contains a sample of creating custom types and type editors for use as CodeSmith properties.
  * Included CodeSmith.rtf User Documentation written by Pete Davis.
  * Fixed bug with ForeignKey collection.  If foreign and primary key column names were different you would get a ArgumentNullException.
  * Fixed bug with custom tool where assemblies in the same directory as the template could not be resolved.
  * Fixed bug where installer wouldn't install if Visual Studio.NET wasn't installed.
Build 2.0.1222:
  * Removed expiration.
  * Created installer.
  * Changed to a build system instead of going on and on forever with beta builds.
  * Dropped support for Visual Studio .NET 2002.  The Visual Studio features will only work with the final release of Visual Studio .NET 2003.
  * Dropped RedRiver from all namespaces.  If you have templates that refer to RedRiver, you will need to update them.
  * Fixed problem where SchemaExplorer would fail if collation was different from the master database.
  * Added view support to SchemaExplorer.
  * Fixed problem with SQL7 compatibility and column descriptions.
  * Added a designer for the DatabaseSchema object.
  * Added Owner property to TableSchema, ViewSchema, and CommandSchema and fixed bug relating to tables, stored procedures and views were assuming dbo as an owner.
  * Updated CollectionGen templates from the latest CollectionGen release.
  * Added a Visual Basic Code Generator Sample (VBCodeGeneratorSample).
  * Added CodeSmithConsole.exe to enable processing a .xml property file from the command line. (CSharpCodeGeneratorSample and VBCodeGeneratorSample folders)
Beta 6:
  * Fixed bug where CodeSmith Explorer context menu was shown for node types other than templates.
  * Added ability to specify the application used to edit templates.  Can be changed in the "CodeSmith.Gui.dll.config" (key "codetemplateexplorercontrol.editapplication") file.
  * Fixed various bugs in SqlCodeTemplate.cs.
  * Changed expiration to 7/1/2003.
  * Seperated the VS.NET addin and custom tool into seperate assemblies.  This should allow the custom tool to run in VS.NET 2002 and 2003.
  * Changed ColumnSchema.DataType and ParameterSchema.DataType to DbType instead of SqlDbType.
  * Fix bug with ColumnSchema.IsPrimaryKeyMember when no primary key exists.
Beta 5:
  * Added a CodeSmith VS.NET custom tool.  This can be used to simulate generics (CSharpCodeGeneratorSample folder).
  * Added a CodeSmith Explorer tool window addin to VS.NET.  This allows you to quickly generate code using drag and drop.
  * Fixed a cosmetic parser bug (parser was eating too much white space after directives).
  * Fixed a bug in drag and drop from CodeSmith Explorer when the code generation was canceled.
Beta 4:
  * Made necessary changes to allow Debugger.Break() to launch a debugger and step through the generated template code.
  * Added ability to double-click compiler errors and go to the spot in the source code where this error was encountered.
  * Added various additional error messages to the compiler.
  * Added Response property to CodeTemplate so you can now use Response.Write statements in template code.
  * Fixed file associations so that .cst files have a CodeSmith icon now.
  * Fixed bug with removing data sources.
  * Added SetProperty and GetProperty helper methods to CodeTemplate.
  * Added a simple latest version check to the about dialog.  Also checks every 15 days (not yet configurable) when CodeSmith Explorer is run.
  * Fixed about dialog showing the wrong build number.
  * Added an example of using CodeSmith.Engine programatically (ConsoleSample folder).
  * Fixed several sample template bugs.
Beta 3:
  * Added ability to save template output to file.
  * Added ability to save compiled template to an assembly.
  * Schema Explorer API documentation is included.
  * Fixed a bug in the schema explorer data source manager when edit is clicked with no data source selected.
  * Fixed a bug with the IsForeignKey property of ColumnSchema and changed the property name to IsForeignKeyMember.
  * Fixed bug for templates that have spaces in their file names.
  * Fixed file association bug.
Beta 2:
  * Fixed a bug with running CodeSmith on .NET 1.0.
  * Added line and column information to the compiled template source tab.
  * Fixed bug where compiled template source would not be shown if there was an error in the template.
Beta 1:
  * Initial public release.
