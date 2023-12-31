﻿using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace WritableOptionsService
{
    public class WritableSvc<T> : IWritableSvc<T> where T : class, new()
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IOptionsMonitor<T> _options;
        private readonly string _section;
        private readonly string _file;

        public WritableSvc( IWebHostEnvironment environment, IOptionsMonitor<T> options, string section, string file)
		{
            _environment = environment;
            _options = options;
            _section = section;
            _file = file;
        }

        public T Value => _options.CurrentValue;

        public T Get(string? name) => _options.Get(name);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8604 // Possible null reference argument.
        public bool Update(Action<T> applyChanges)
        {
            bool resultError = false;

            try
            {
                var fileProvider = _environment.ContentRootFileProvider;
                var fileInfo = fileProvider.GetFileInfo(_file);
                var physicalPath = fileInfo.PhysicalPath;

                var jObject = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(path: physicalPath));
                var sectionObject = jObject.TryGetValue(_section, value: out JToken section) ?
                    JsonConvert.DeserializeObject<T>(section.ToString()) : (Value ?? new T());

                applyChanges(sectionObject);

                jObject[_section] = JObject.Parse(JsonConvert.SerializeObject(sectionObject));
                File.WriteAllText(physicalPath, JsonConvert.SerializeObject(jObject, Formatting.Indented));

                resultError = false;
            }
            catch (Exception ex)
            {
                Log.Error("An error occurred while seeding the database : {Error} * {StackTrace} * {InnerException} * {Source}",
                    ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                resultError = true;
            }

            return resultError;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
        }
    }
}

