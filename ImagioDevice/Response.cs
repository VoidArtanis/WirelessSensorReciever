using System;

namespace ImagioDevice
{
   public class Response
    {
        public string FunctionIndex { get; set; }
        public string Value { get; set; }

       public Response(string funcIndex, string val)
       {
           FunctionIndex = funcIndex;
           Value = val;
       }
       
        public static Response GetResponse(Message msg)
        {
            var splits = msg.Command.Split('=');
            if (splits.Length == 2)
            {
                return new Response(splits[0].Replace(";", ""), splits[1].Replace(";", "")); ;
            }
            return null;
        }
    }
}
