using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsMotors.DataClasses
{
	public class MspSetRawRcResponse : MSPResponse
	{
		public static MspSetRawRcResponse FromByteArray(byte[] payload)
		{
			return new MspSetRawRcResponse(); 
		}

		public override string ToString()
		{
			return $"MspSetRawRcResponse returned successfully";
		}
	}
}
