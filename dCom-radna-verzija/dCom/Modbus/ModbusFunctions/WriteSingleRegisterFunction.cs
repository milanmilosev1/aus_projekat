using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters writeParams = (ModbusWriteCommandParameters)CommandParameters;

            byte[] request = new byte[12];
            int offset = 0;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.TransactionId)), 0, request, offset, 2);
            offset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.ProtocolId)), 0, request, offset, 2);
            offset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.Length)), 0, request, offset, 2);
            offset += 2;

            request[offset++] = writeParams.UnitId;
            request[offset++] = writeParams.FunctionCode;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.OutputAddress)), 0, request, offset, 2);
            offset += 2;

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)writeParams.Value)), 0, request, offset, 2);
            offset += 2;

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            var result = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if ((response[7] & 0x80) != 0)
            {
                HandeException(response[8]);
            }

            ushort address = (ushort)((response[8] << 8) | response[9]);
            ushort value = (ushort)((response[10] << 8) | response[11]);

            result.Add(
                new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address),
                value);

            return result;
        }
    }
}