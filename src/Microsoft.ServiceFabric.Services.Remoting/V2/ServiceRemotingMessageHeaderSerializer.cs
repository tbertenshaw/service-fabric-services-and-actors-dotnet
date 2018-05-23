// ------------------------------------------------------------
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT License (MIT).See License.txt in the repo root for license information.
// ------------------------------------------------------------

namespace Microsoft.ServiceFabric.Services.Remoting.V2
{
    using System.Runtime.Serialization;
    using System.Xml;
    using Microsoft.ServiceFabric.Services.Remoting.V2.Messaging;

    internal class ServiceRemotingMessageHeaderSerializer : IServiceRemotingMessageHeaderSerializer
    {
        private readonly IBufferPoolManager bufferPoolManager;
        private readonly DataContractSerializer requestHeaderSerializer;
        private readonly DataContractSerializer responseHeaderSerializer;

        public ServiceRemotingMessageHeaderSerializer(
            IBufferPoolManager bufferPoolManager)
            : this(
                bufferPoolManager,
                new DataContractSerializer(
                    typeof(IServiceRemotingRequestMessageHeader),
                    new DataContractSerializerSettings()
                    {
                        MaxItemsInObjectGraph = int.MaxValue,
                        KnownTypes = new[] { typeof(ServiceRemotingRequestMessageHeader) },
                    }))
        {
        }

        public ServiceRemotingMessageHeaderSerializer(
            IBufferPoolManager bufferPoolManager,
            DataContractSerializer headerRequestSerializer)
        {
            this.bufferPoolManager = bufferPoolManager;
            this.requestHeaderSerializer = headerRequestSerializer;
            this.responseHeaderSerializer = new DataContractSerializer(
                typeof(IServiceRemotingResponseMessageHeader),
                new DataContractSerializerSettings()
                {
                    MaxItemsInObjectGraph = int.MaxValue,
                    KnownTypes = new[] { typeof(ServiceRemotingResponseMessageHeader) },
                });
        }

        public IMessageHeader SerializeRequestHeader(IServiceRemotingRequestMessageHeader serviceRemotingRequestMessageHeader)
        {
            if (serviceRemotingRequestMessageHeader == null)
            {
                return null;
            }

            using (var stream = new SegmentedPoolMemoryStream(this.bufferPoolManager))
            {
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
                {
                    this.requestHeaderSerializer.WriteObject(writer, serviceRemotingRequestMessageHeader);
                    writer.Flush();
                    return new OutgoingMessageHeader(stream.GetBuffers());
                }
            }
        }

        public IServiceRemotingRequestMessageHeader DeserializeRequestHeaders(IMessageHeader messageHeader)
        {
            if ((messageHeader == null) || (messageHeader.GetReceivedBuffer() == null) ||
                (messageHeader.GetReceivedBuffer().Length == 0))
            {
                return null;
            }

            using (var reader = XmlDictionaryReader.CreateBinaryReader(
                messageHeader.GetReceivedBuffer(),
                XmlDictionaryReaderQuotas.Max))
            {
                return (IServiceRemotingRequestMessageHeader)this.requestHeaderSerializer.ReadObject(reader);
            }
        }

        public IMessageHeader SerializeResponseHeader(IServiceRemotingResponseMessageHeader serviceRemotingResponseMessageHeader)
        {
            if (serviceRemotingResponseMessageHeader == null || serviceRemotingResponseMessageHeader.CheckIfItsEmpty())
            {
                return null;
            }

            using (var stream = new SegmentedPoolMemoryStream(this.bufferPoolManager))
            {
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
                {
                    this.responseHeaderSerializer.WriteObject(writer, serviceRemotingResponseMessageHeader);
                    writer.Flush();
                    return new OutgoingMessageHeader(stream.GetBuffers());
                }
            }
        }

        public IServiceRemotingResponseMessageHeader DeserializeResponseHeaders(IMessageHeader messageHeader)
        {
            if ((messageHeader == null) || (messageHeader.GetReceivedBuffer() == null) ||
                (messageHeader.GetReceivedBuffer().Length == 0))
            {
                return null;
            }

            using (var reader = XmlDictionaryReader.CreateBinaryReader(
                messageHeader.GetReceivedBuffer(),
                XmlDictionaryReaderQuotas.Max))
            {
                return (IServiceRemotingResponseMessageHeader)this.responseHeaderSerializer.ReadObject(reader);
            }
        }
    }
}
