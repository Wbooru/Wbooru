using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Wbooru.Models.Gallery.Annotation
{
    public delegate bool OnClickCallBack(object property_object, object state);

    public class DisplayClickActionAttribute : Attribute
    {
        public DisplayClickActionAttribute(string call_back_name,object state = null)
        {
            MethodName = call_back_name;
            State = state;
        }

        public string MethodName { get; }
        public object State { get; }
        private MethodInfo Info { get; set; }

        public bool RemoteCallBack(object method_host, object o)
        {
            if (Info == null)
                Init(method_host);

            return Info?.Invoke(method_host, new[] { o, State}) is bool b ? b : true;
        }

        private void Init(object method_host)
        {
            Info = method_host.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
    }
}
