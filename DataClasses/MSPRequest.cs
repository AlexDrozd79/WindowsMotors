using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public abstract class MSPRequest
	{
		public abstract byte[] ToByteArray(); 
	}
}
