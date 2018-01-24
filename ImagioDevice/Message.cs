using System;

namespace ImagioDevice
{
   public class Message
    {
       private Response _response ;
       public string Command { get; set; }
        public CommandType Type { get; set; }

       public Response Response
       {
           get { return _response ?? (_response = Response.GetResponse(this)); }
           set { _response = value; }
       }

       public Message(string command, CommandType type)
       {
           Command = command;
           Type = type;
       }

        public Message()
        {

        }

        public static Message ParseMessage(string command)
        {
            try
            {
                var splits = command.Split(':');
                if (splits.Length == 2)
                {
                    return new Message(splits[1], (CommandType) Enum.Parse(typeof (CommandType), splits[0]));
                }
                else
                {
                    return new Message(splits[0], CommandType.Unknown);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(command);
                return null;
            }
        } 

    }
}
