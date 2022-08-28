using System;
using Microsoft.CodeAnalysis;

namespace NDiff.Services.Analyzers.AttributeAnalyzers.Attributes
{
    public abstract class Attribute
    {
        protected AttributeData[] AttributeData { get; }

        protected Attribute(AttributeData[] attributeData)
        {
            AttributeData = attributeData;
        }

        /// <summary>
        /// Analyzes the attributes <see cref="AttributeData"/>.
        /// </summary>
        protected abstract void Analyze();

        /// <summary>
        /// It analyzes the attributes based on the implementation of <see cref="Analyze"/>.
        /// </summary>
        public void AnalyzeAttributes()
        {
            if (!IsValid())
            {
                return;
            }

            try
            {
                Analyze();
            }
            catch (Exception ex)
            {
                // ignore
            }
        }

        /// <summary>
        /// Checks if the <see cref="AttributeData"/> is valid or not.
        /// </summary>
        /// <returns>True if valid; otherwise, false.</returns>
        protected abstract bool IsValid();
    }
}