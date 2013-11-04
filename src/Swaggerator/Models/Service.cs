﻿/*
 * Copyright (c) 2013 Digimarc Corporation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Service.cs : Service model for serialization.
 */

using System.Runtime.Serialization;

namespace Swaggerator.Models
{
	[DataContract]
	internal class Service
	{
		[DataMember]
		public string path { get; set; }
		[DataMember(EmitDefaultValue = false)]
		public string description { get; set; }
	}
}