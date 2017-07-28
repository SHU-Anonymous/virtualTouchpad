using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;

namespace SendData
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NamedPipeServerStream pipeStream = new NamedPipeServerStream("testpipe"))
            {
                pipeStream.WaitForConnection();

                using (StreamWriter writer = new StreamWriter(pipeStream))
                {
                    writer.AutoFlush = true;
                    string temp;

                    while ((temp = Console.ReadLine()) != "stop")
                    {
                        writer.WriteLine(temp);
                        writer.WriteLine(temp);
                        writer.WriteLine(temp);
                    }
                }
            }
        }
    }
}
