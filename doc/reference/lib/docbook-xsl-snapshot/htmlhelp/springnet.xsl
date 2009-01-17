<?xml version="1.0"?>
<!--
    This is the XSL HTML Help stylesheet for the Spring reference Documentation.
-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:doc="http://nwalsh.com/xsl/documentation/1.0"
                xmlns:exsl="http://exslt.org/common"
                xmlns:set="http://exslt.org/sets"
                exclude-result-prefixes="doc exsl set"
                version="1.0">

  <xsl:import href="../html/springnet.xsl"/>

  <!--###################################################
                         General
    ################################################### -->
  <xsl:param name="base.dir">htmlhelp/</xsl:param>
  <xsl:param name="manifest.in.base.dir">htmlhelp/</xsl:param>
  
  <xsl:include href="htmlhelp-common.xsl"/>

</xsl:stylesheet>
