This Docbook documentation was taken from the Spring.Java project. The
documentation can be generated using Java Ant. The targets are:

* docpdf        - generates the PDF documentation
* dochtml       - generates the HTML documentation
* dochtmlsingle - generates single page HTML documentation
* clean         - clean any output directories for docs

To generate documentation, you need to include a lot of libraries, which
haven't been added to CVS because they're simply too big. The libraries can
be downloaded from sourceforge or from
http://www.jteam.nl/spring/reference-libraries.zip.  Download them, and
unzip the zip into reference/lib.  Then, the targets should work.

Thanks to Spring.Java and Hibernate, for providing the skeleton for DocBook
documentation!

choy@rcn.com

