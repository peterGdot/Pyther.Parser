namespace Pyther.Parser.INI
{
    public struct INIConfig
    {
        public bool UseCaseSensitive = false;
        public bool IgnoreEmptyLines = true;
        public char KeyValueReadDelimiter = '=';
        public string KeyValueWriteDelimiter = " = ";
        public char[] CommentIndication = new char[] { ';', '#' };
        public string DefaultComment = "; ";
        
        public INIConfig()
        {

        }
    }
}
