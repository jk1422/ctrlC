using Colossal.UI.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ctrlC.Utils
{
    public class StringListWriter : IWriter<List<List<string>>>
    {
        public void Write(IJsonWriter writer, List<List<string>> value)
        {
            writer.ArrayBegin(value.Count);
            foreach (var list in value)
            {
                writer.ArrayBegin(list.Count);
                foreach (var item in list)
                {
                    writer.Write(item);
                }
                writer.ArrayEnd();
            }
            writer.ArrayEnd();
        }
    }

    public class StringArrayWriter : IWriter<string[]>
    {
        public void Write(IJsonWriter writer, string[] value)
        {
            writer.ArrayBegin(value.Length);
            foreach (var item in value)
            {
                writer.Write(item);
            }
            writer.ArrayEnd();
        }
    }

}
