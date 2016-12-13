using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public partial class Addresser
    {
        public override string ToString()
        {
            return String.Format("{0}\n{1}", ContactName, Position);
        }
    }

    public partial class Topic
    {
        public override string ToString()
        {
            return this.Name;
        }
    } 
}
