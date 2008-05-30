<?xml version="1.0"?>
<!--
   Copyright 2002-2006 the original author or authors.
 
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
 
         http://www.apache.org/licenses/LICENSE-2.0
 
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
-->

<!--
   Effects the transformation of the various Spring schema definitions to DocBook XML.

   Author: Rick Evans
-->
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xsd="http://www.w3.org/2001/XMLSchema"
                xmlns:xalan="http://xml.apache.org/xalan">

    <xsl:output
            method="xml"
            indent="yes"
            xalan:indent-amount="3"
            omit-xml-declaration="no"/>

    <xsl:param name="title"/>

    <xsl:template match="xsd:schema">
        <xsl:element name="appendix">
            <xsl:attribute name="id">
                <xsl:value-of select="$title"/>
            </xsl:attribute>
            <xsl:element name="title">
                <xsl:element name="literal">
                    <xsl:value-of select="$title"/>
                </xsl:element>
            </xsl:element>

            <xsl:element name="section">
                <xsl:attribute name="id">
                    <xsl:value-of select="$title"/>
                    <xsl:text>-intro</xsl:text>
                </xsl:attribute>
                <xsl:element name="title">
                    <xsl:text>Introduction</xsl:text>
                </xsl:element>
            </xsl:element>

            <xsl:element name="para">
                <xsl:text>This appendix describes the </xsl:text>
                <xsl:element name="literal">
                    <xsl:value-of select="$title"/>
                </xsl:element>
                <xsl:text> schema.</xsl:text>
            </xsl:element>

            <xsl:element name="itemizedlist">
                <xsl:apply-templates select="xsd:element" mode="mini-toc">
                    <xsl:sort select="name" order="ascending"/>
                </xsl:apply-templates>
            </xsl:element>

            <xsl:apply-templates select="xsd:element">
                <xsl:sort select="name" order="ascending"/>
            </xsl:apply-templates>

        </xsl:element>
    </xsl:template>

    <!--
    creates the elements of an <itemizedlist/> containing (hyper)links
    to the actual content
    -->
    <xsl:template match="xsd:element" mode="mini-toc">
        <xsl:element name="listitem">
            <xsl:element name="xref">
                <xsl:attribute name="linkend">
                    <xsl:call-template name="generate.id">
                        <xsl:with-param name="id">
                            <xsl:value-of select="@name"/>
                        </xsl:with-param>
                    </xsl:call-template>
                </xsl:attribute>
            </xsl:element>
        </xsl:element>
    </xsl:template>

    <xsl:template match="xsd:element">
        <xsl:element name="section">
            <xsl:attribute name="id">
                <xsl:call-template name="generate.id">
                    <xsl:with-param name="id">
                        <xsl:value-of select="@name"/>
                    </xsl:with-param>
                </xsl:call-template>
            </xsl:attribute>
            <xsl:element name="title">
                <xsl:text>The </xsl:text>
                <xsl:element name="literal">
                    <xsl:value-of select="@name"/>
                </xsl:element>
                <xsl:text> element</xsl:text>
            </xsl:element>
            <xsl:element name="para">
                <xsl:text>[TODO : insert the description of the element here]</xsl:text>
            </xsl:element>
        </xsl:element>
    </xsl:template>

    <xsl:template name="generate.id">
        <xsl:param name="id"/>
        <xsl:value-of select="$title"/>.<xsl:value-of select="$id"/>
    </xsl:template>

</xsl:stylesheet>
