﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using Swaggerator.Models;
using Swaggerator.Attributes;

namespace Swaggerator
{
	internal class Mappers
	{
		/// <summary>
		/// Find methods of the supplied type which have WebGet or WebInvoke attributes.
		/// </summary>
		/// <param name="serviceType">The implementation type to search.</param>
		/// <param name="typeStack">Types to be documented in the models section.</param>
		internal static IEnumerable<Method> FindMethods(Type serviceType, Stack<Type> typeStack)
		{
			List<Tuple<string, Operation>> operations = new List<Tuple<string, Operation>>();

			//search all interfaces for this type for potential DataContracts, and build a set of operations
			Type[] interfaces = serviceType.GetInterfaces();
			foreach (Type i in interfaces)
			{
				Attribute dc = i.GetCustomAttribute(typeof(ServiceContractAttribute));
				if (dc != null)
				{
					//found a DataContract, now get a service map and inspect the methods for WebGet/WebInvoke
					InterfaceMapping map = serviceType.GetInterfaceMap(i);
					operations.AddRange(GetOperations(map, typeStack));
				}
			}

			List<Method> methods = new List<Method>();

			//go through the discovered Operations, and combine any like Uri's into Methods.
			foreach (Tuple<string, Operation> t in operations)
			{
				Method method = (from m in methods
									  where m.path.Equals(t.Item1)
									  select m).FirstOrDefault();
				if (method == null)
				{
					method = new Method { path = t.Item1 };
					methods.Add(method);
				}
				method.operations.Add(t.Item2);
			}

			return methods;
		}

		private static IEnumerable<Tuple<string, Operation>> GetOperations(InterfaceMapping map, Stack<Type> typeStack)
		{
			for (int index = 0; index < map.InterfaceMethods.Count(); index++)
			{
				MethodInfo implementation = map.TargetMethods[index];
				MethodInfo declaration = map.InterfaceMethods[index];

				//if the method is marked Hidden anywhere, skip it
				if (implementation.GetCustomAttribute<HiddenAttribute>() != null ||
					declaration.GetCustomAttribute<HiddenAttribute>() != null) { continue; }

				//find the WebGet/Invoke attributes, or skip if neither is present
				WebGetAttribute wg = declaration.GetCustomAttribute<WebGetAttribute>();
				WebInvokeAttribute wi = declaration.GetCustomAttribute<WebInvokeAttribute>();
				if (wg == null && wi == null) { continue; }

				string httpMethod = (wi == null) ? "GET" : wi.Method;
				string uriTemplate = (wi == null) ? wg.UriTemplate ?? "" : wi.UriTemplate ?? "";

				//implementation description overrides interface description
				string description =
					Helpers.GetCustomAttributeValue<string, DescriptionAttribute>(implementation, "Description") ??
					Helpers.GetCustomAttributeValue<string, DescriptionAttribute>(declaration, "Description") ??
					"";

				string summary =
				Helpers.GetCustomAttributeValue<string, OperationSummaryAttribute>(implementation, "Summary") ??
				Helpers.GetCustomAttributeValue<string, OperationSummaryAttribute>(declaration, "Summary") ??
				"";

				Operation operation = new Operation
				{
					httpMethod = httpMethod,
					nickname = declaration.Name + httpMethod,
					responseClass = Helpers.MapSwaggerType(declaration.ReturnType, typeStack),
					//TODO add mechanism to make this somewhat configurable
					summary = summary,
					notes = description
				};

				operation.errorResponses.AddRange(GetResponseCodes(map.TargetMethods[index]));
				operation.errorResponses.AddRange(from r in GetResponseCodes(map.InterfaceMethods[index])
															 where !operation.errorResponses.Any(c => c.code.Equals(r.code))
															 select r);

				ParameterInfo[] parameters = declaration.GetParameters();
				foreach (ParameterInfo parameter in parameters)
				{
					Parameter parm = new Parameter
					{
						name = parameter.Name,
						allowMultiple = false,
						required = true,
						dataType = Helpers.MapSwaggerType(parameter.ParameterType, typeStack)
					};
					if (uriTemplate.Contains("{" + parameter.Name + "}"))
					{
						parm.paramType = uriTemplate.IndexOf("{" + parameter.Name + "}") < uriTemplate.IndexOf("?") ? "path" : "query";
					}
					else
					{
						parm.paramType = "body";
					}
					operation.parameters.Add(parm);
				}

				yield return new Tuple<string, Operation>(uriTemplate, operation);
			}
		}

		private static IEnumerable<ResponseCode> GetResponseCodes(MethodInfo methodInfo)
		{
			return methodInfo.GetCustomAttributes<ResponseCodeAttribute>().Select(rca => new ResponseCode
			{
				code = rca.Code,
				reason = rca.Description
			});
		}
	}
}
