using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.IO;

namespace ReceiveData
{
    class Program
    {
        static void Main(string[] args)
        {
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream("testpipe"))
            {
                pipeStream.Connect();
                //在client读取server端写的数据
                using (StreamReader rdr = new StreamReader(pipeStream))
                {
                    string temp;
                    while ((temp = rdr.ReadLine()) != "")
                    {
                        Console.WriteLine("{0}:{1}", DateTime.Now, temp);
                    }
                }
            }

            Console.Read();

        }
    }
}
