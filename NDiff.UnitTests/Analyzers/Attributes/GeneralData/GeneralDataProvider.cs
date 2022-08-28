using System.Linq;

namespace NDiff.UnitTests.Analyzers.Attributes.GeneralData
{
    public class GeneralDataProvider
    {
        protected const string TestClassName = "AttributesClassTest";

        protected string CreateTestClassWithAttribute(string classAttributeName = null,
            string methodAttributeName = null, string classAttributeBody = null, string methodAttributeBody = null)
        {
            var classTextAttribute = "";
            if (classAttributeName is not null)
                classTextAttribute = $@"[{classAttributeName}(" + classAttributeBody + ")]";

            var methodTextAttribute = "";
            if (methodAttributeName is not null)
                methodTextAttribute = $@"[{methodAttributeName}(" + methodAttributeBody + ")]";

            return $@"
                    using System;
                    using System.Collections;
                    using System.Collections.Generic;
                    using System.ComponentModel.DataAnnotations;
                    using System.Linq;
                    using System.Net.Mime;
                    using Microsoft.AspNetCore.Http;
                    using Microsoft.AspNetCore.Mvc;
                    using Microsoft.Extensions.Logging;

                    namespace TestNamespace
                    {{ 
                        {classTextAttribute}
                        public class {TestClassName} : ControllerBase
                        {{
                            {methodTextAttribute}
                            public string CreateAsync([FromQuery][Required] List<List<Dictionary<string, BB>>> a,[FromQuery] BB dd1, 
                                        [FromQuery]int[][]asd, [FromQuery] ArrayList deed, Hashtable hash,[FromQuery] HashSet<string> fff, 
                                        string cc, [FromQuery] long ff, short ff1)
                            {{

                                return ""a"";
                            }}
                        }}
                    }}
                ";
        }
    }
}