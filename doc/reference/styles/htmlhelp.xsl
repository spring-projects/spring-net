<?xml version="1.0"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:doc="http://nwalsh.com/xsl/documentation/1.0"
                xmlns:exsl="http://exslt.org/common"
                xmlns:set="http://exslt.org/sets"
		version="1.0"
                exclude-result-prefixes="doc exsl set">

<!-- ********************************************************************
     $Id: htmlhelp.xsl,v 1.1 2005/08/28 03:49:20 markpollack Exp $
     ******************************************************************** 

     This file is used by htmlhelp.xsl if you want to generate source
     files for HTML Help.  It is based on the XSL DocBook Stylesheet
     distribution (especially on JavaHelp code) from Norman Walsh.

     ******************************************************************** -->
<!--
<xsl:import href="../html/chunk.xsl"/>
-->

<xsl:import href="html_chunk.xsl"/>

<xsl:param name="shade.verbatim" select="1"></xsl:param>
<xsl:attribute-set name="shade.verbatim.style">
  <xsl:attribute name="border">1</xsl:attribute>
  <xsl:attribute name="bgcolor">#F4F4F4</xsl:attribute>
</xsl:attribute-set>


<xsl:param name="manifest.in.base.dir" select="0"/>
<xsl:param name="htmlhelp.alias.file" select="'alias.h'"/>
<xsl:param name="htmlhelp.autolabel" select="0"/>
<xsl:param name="htmlhelp.button.back" select="1"/>
<xsl:param name="htmlhelp.button.forward" select="0"/>
<xsl:param name="htmlhelp.button.hideshow" select="1"/>
<xsl:param name="htmlhelp.button.home" select="0"/>
<xsl:param name="htmlhelp.button.home.url"/>
<xsl:param name="htmlhelp.button.jump1" select="0"/>
<xsl:param name="htmlhelp.button.jump1.title" select="'User1'"/>
<xsl:param name="htmlhelp.button.jump1.url"/>
<xsl:param name="htmlhelp.button.jump2" select="0"/>
<xsl:param name="htmlhelp.button.jump2.title" select="'User2'"/>
<xsl:param name="htmlhelp.button.jump2.url"/>
<xsl:param name="htmlhelp.button.locate" select="0"/>
<xsl:param name="htmlhelp.button.next" select="1"/>
<xsl:param name="htmlhelp.button.options" select="1"/>
<xsl:param name="htmlhelp.button.prev" select="1"/>
<xsl:param name="htmlhelp.button.print" select="1"/>
<xsl:param name="htmlhelp.button.refresh" select="0"/>
<xsl:param name="htmlhelp.button.stop" select="0"/>
<xsl:param name="htmlhelp.button.zoom" select="0"/>
<xsl:param name="htmlhelp.chm" select="'htmlhelp.chm'"/>
<xsl:param name="htmlhelp.default.topic" select="''"/>
<xsl:param name="htmlhelp.display.progress" select="1"/>
<xsl:param name="htmlhelp.encoding" select="'iso-8859-1'"/>
<xsl:param name="htmlhelp.enhanced.decompilation" select="0"/>
<xsl:param name="htmlhelp.enumerate.images" select="0"/>
<xsl:param name="htmlhelp.force.map.and.alias" select="0"/>
<xsl:param name="htmlhelp.hhc.binary" select="1"/>
<xsl:param name="htmlhelp.hhc.folders.instead.books" select="1"/>
<xsl:param name="htmlhelp.hhc" select="'toc.hhc'"/>
<xsl:param name="htmlhelp.hhc.section.depth" select="5"/>
<xsl:param name="htmlhelp.hhc.show.root" select="1"/>
<xsl:param name="htmlhelp.hhc.width"/>
<xsl:param name="htmlhelp.hhk" select="'index.hhk'"/>
<xsl:param name="htmlhelp.hhp" select="'htmlhelp.hhp'"/>
<xsl:param name="htmlhelp.hhp.tail"/>
<xsl:param name="htmlhelp.hhp.window" select="'Main'"/>
<xsl:param name="htmlhelp.hhp.windows"/>
<xsl:param name="htmlhelp.map.file" select="'context.h'"/>
<xsl:param name="htmlhelp.only" select="0"/>
<xsl:param name="htmlhelp.remember.window.position" select="0"/>
<xsl:param name="htmlhelp.show.advanced.search" select="0"/>
<xsl:param name="htmlhelp.show.favorities" select="0"/>
<xsl:param name="htmlhelp.show.menu" select="0"/>
<xsl:param name="htmlhelp.show.toolbar.text" select="1"/>
<xsl:param name="htmlhelp.title" select="''"/>
<xsl:param name="htmlhelp.use.hhk" select="0"/>
<xsl:param name="htmlhelp.window.geometry"/>

<xsl:template name="href.target.with.base.dir">
  <xsl:param name="object" select="."/>
  <xsl:if test="$manifest.in.base.dir = 0">
    <xsl:value-of select="$base.dir"/>
  </xsl:if>
  <xsl:call-template name="href.target">
    <xsl:with-param name="object" select="$object"/>
  </xsl:call-template>
</xsl:template>
    

<xsl:include href="htmlhelp-common.xsl"/>

</xsl:stylesheet>
