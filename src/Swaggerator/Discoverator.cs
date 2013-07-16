﻿using Swaggerator.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.ServiceModel.Activation;

namespace Swaggerator
{
	[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
	public class Discoverator : IDiscoverator
	{
		public Stream GetServices()
		{
			Models.ServiceList serviceList = new Models.ServiceList
			{
				basePath = HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + "/api-docs.json",
				swaggerVersion = Globals.SWAGGER_VERSION,
				apiVersion = "No Swaggerized assemblies."
			};

			Assembly[] allAssm = AppDomain.CurrentDomain.GetAssemblies();

			bool foundAssembly = false;
			foreach (Assembly assm in allAssm)
			{
				IEnumerable<Models.Service> services = GetDiscoveratedServices(assm);
				if (services.Any())
				{
					if (foundAssembly) { serviceList.apiVersion = "Multiple Assemblies"; }
					else
					{
						foundAssembly = true;
						serviceList.apiVersion = assm.GetName().Version.ToString();
					}
					serviceList.apis.AddRange(services);
				}
			}

			MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(serviceList)));

			return stream;
		}

		private static IEnumerable<Models.Service> GetDiscoveratedServices(Assembly assembly)
		{
			IEnumerable<TypeInfo> types;
			try
			{
				types = assembly.DefinedTypes;
			}
			catch (ReflectionTypeLoadException ex)
			{
				//couldn't load this assembly - probably a non-issue
				yield break;
			}

			foreach (TypeInfo ti in types)
			{
				SwaggeratedAttribute da = ti.GetCustomAttribute<SwaggeratedAttribute>();
				if (da != null)
				{
					DescriptionAttribute descAttr = ti.GetCustomAttribute<DescriptionAttribute>();
					Models.Service service = new Models.Service
					{
						path = da.LocalPath,
						description = (descAttr == null) ? da.Description : descAttr.Description
					};
					yield return service;
				}
			}
		}

		private static Type FindServiceTypeByPath(string servicePath)
		{
			Assembly[] allAssm = AppDomain.CurrentDomain.GetAssemblies();
			foreach (Assembly assembly in allAssm)
			{
				foreach (TypeInfo ti in assembly.DefinedTypes)
				{
					SwaggeratedAttribute da = ti.GetCustomAttribute<SwaggeratedAttribute>();
					if (da != null && da.LocalPath == servicePath)
					{
						return ti.AsType();
					}
				}
			}
			return null;
		}

		public Stream GetServiceDetails(string servicePath)
		{
			Type serviceType = FindServiceTypeByPath(string.Format("/{0}", servicePath));

			Stack<Type> typeStack = new Stack<Type>();

			string api = Serializers.WriteApi(servicePath, serviceType, typeStack);

			MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(api));
			return ms;
		}
	}
}