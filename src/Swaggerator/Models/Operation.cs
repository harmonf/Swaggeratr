﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Swaggerator.Models
{
    [DataContract]
    internal class Operation
    {
        public Operation()
        {
            parameters = new List<Parameter>();
            errorResponses = new List<ResponseCode>();
            accepts = new List<string>();
            produces = new List<string>();
        }

        [DataMember]
        public string httpMethod { get; set; }
        [DataMember]
        public string nickname { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public OperationItems items { get; set; }
        [DataMember]
        public List<Parameter> parameters { get; set; }
        [DataMember]
        public string summary { get; set; }
        [DataMember]
        public string notes { get; set; }
        [DataMember]
        public List<ResponseCode> errorResponses { get; set; }
        [DataMember]
        public List<string> accepts { get; set; }
        [DataMember]
        public List<string> produces { get; set; }
    }

    [DataContract]
    internal class OperationItems
    {
        [DataMember(Name = "$ref")]
        public string Ref { get; set; }
    }
}