using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public static class Extensions_Routing
{

	const string RouteParseRegex = @"{([^}]+)}";

	public static Uri GetUri(this Controller controller, MethodBase method, Dictionary<string, object> parameters = null)
	{
		string routeUnformatted = "";
		var prefixAttribute = method.DeclaringType.GetCustomAttribute<RouteAttribute>();
		if (prefixAttribute != null)
		{
			routeUnformatted += prefixAttribute.Template;
		}

		var routeAttribute = method.GetCustomAttribute<RouteAttribute>();

		if (routeAttribute != null)
		{
			routeUnformatted += $"/{routeAttribute.Template}";
		}

		//have full route, now parse by regex groups

		var patterns = Regex.Matches(routeUnformatted, RouteParseRegex);

		foreach (var group in patterns)
		{
			Match match = group as Match;
			string filtered = match.Value.Replace("{", "").Replace("}", "");
			string[] split = filtered.Split(new char[] { ':' });

			string variable = split[0];

			if (!parameters.ContainsKey(variable))
			{
				throw new Exception($"{variable} is missing from passed in parameters. Please check your route.");
			}

			object param = parameters[variable];
			routeUnformatted = routeUnformatted.Replace(match.Value, param.ToString());
		}
		return new Uri(routeUnformatted, UriKind.Relative);
	}
}