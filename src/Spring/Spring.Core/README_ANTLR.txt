Notes on code merge into Spring.Core:


28-07-2009
----------

*) Added codebase version 2.7.7 of antlr
*) had to move <global>::SupportClass to namespace "antlr"
*) Replaced Method antlr.InputBuffer.LA(int i) with

        public virtual char LA(int i)
        {
            fill(i);
            if ((this.markerOffset + i) <= this.queue.Count)
            {
                return (char)this.queue[(this.markerOffset + i) - 1];
            }
            return CharScanner.EOF_CHAR;
        }
*) renamed namespaces "antlr.*" to "Spring.Expressions.Parser.antrl.*"
