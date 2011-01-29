<?xml version='1.0'?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
		xmlns:xslthl="http://xslthl.sf.net"
                exclude-result-prefixes="xslthl"
                version='1.0'>

<!-- ********************************************************************
     $Id: highlight.xsl 8093 2008-07-21 16:43:53Z kosek $
     ********************************************************************

     This file is part of the XSL DocBook Stylesheet distribution.
     See ../README or http://docbook.sf.net/release/xsl/current/ for
     and other information.

     ******************************************************************** -->

<xsl:template match='xslthl:keyword' mode="xslthl">
  <span style="color: #0000FF"><xsl:apply-templates mode="xslthl"/></span>
</xsl:template>

<xsl:template match='xslthl:string' mode="xslthl">
  <span style="color: #A31515"><xsl:apply-templates mode="xslthl"/></span>
</xsl:template>

<xsl:template match='xslthl:comment' mode="xslthl">
  <i style="color: #008000"><xsl:apply-templates mode="xslthl"/></i>
</xsl:template>

<xsl:template match='xslthl:directive' mode="xslthl">
  <xsl:apply-templates mode="xslthl"/>
</xsl:template>

<xsl:template match='xslthl:tag' mode="xslthl">
  <span style="color: #A31515"><xsl:apply-templates mode="xslthl"/></span>
</xsl:template>

<xsl:template match='xslthl:attribute' mode="xslthl">
  <span style="color: #FF0000"><xsl:apply-templates mode="xslthl"/></span>
</xsl:template>

<xsl:template match='xslthl:value' mode="xslthl">
  <span style="color: #0000FF"><xsl:apply-templates mode="xslthl"/></span>
</xsl:template>

<xsl:template match='xslthl:html' mode="xslthl">
  <span style="color: #A31515"><xsl:apply-templates mode="xslthl"/></span>
</xsl:template>

<xsl:template match='xslthl:xslt' mode="xslthl">
  <b style="color: #0066FF"><xsl:apply-templates mode="xslthl"/></b>
</xsl:template>

<!-- Not emitted since XSLTHL 2.0 -->
<xsl:template match='xslthl:section' mode="xslthl">
  <b><xsl:apply-templates mode="xslthl"/></b>
</xsl:template>

<xsl:template match='xslthl:number' mode="xslthl">
  <xsl:apply-templates mode="xslthl"/>
</xsl:template>

<xsl:template match='xslthl:annotation' mode="xslthl">
  <i style="color: gray"><xsl:apply-templates mode="xslthl"/></i>
</xsl:template>

<!-- Not sure which element will be in final XSLTHL 2.0 -->
<xsl:template match='xslthl:doccomment|xslthl:doctype' mode="xslthl">
  <b><xsl:apply-templates mode="xslthl"/></b>
</xsl:template>

</xsl:stylesheet>