// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if !CORECLR
using System.ComponentModel.Composition;
#endif
using System.Globalization;
using System.Management.Automation.Language;
using Microsoft.Windows.PowerShell.ScriptAnalyzer.Generic;

namespace Microsoft.Windows.PowerShell.ScriptAnalyzer.BuiltinRules
{
    /// <summary>
    /// AvoidLongLines: Checks for lines longer than 120 characters
    /// </summary>
#if !CORECLR
    [Export(typeof(IScriptRule))]
#endif
    public class AvoidLongLines : ConfigurableRule
    {
        /// <summary>
        /// Construct an object of AvoidLongLines type.
        /// </summary>
        public AvoidLongLines() : base()
        {
            // Enable the rule by default
            Enable = false;
        }

        [ConfigurableRuleProperty(defaultValue: 120)]
        public int LineLength { get; set; }
        /// <summary>
        /// Analyzes the given ast to find violations.
        /// </summary>
        /// <param name="ast">AST to be analyzed. This should be non-null</param>
        /// <param name="fileName">Name of file that corresponds to the input AST.</param>
        /// <returns>A an enumerable type containing the violations</returns>
        public override IEnumerable<DiagnosticRecord> AnalyzeScript(Ast ast, string fileName)
        {
            if (ast == null)
            {
                throw new ArgumentNullException("ast");
            }

            var diagnosticRecords = new List<DiagnosticRecord>();

            string[] lines = Regex.Split(ast.Extent.Text, @"\r?\n");

            for (int lineNumber = 0; lineNumber < lines.Length; lineNumber++)
            {
                var line = lines[lineNumber];

                if (line.Length > LineLength)
                {
                    var startLine = lineNumber + 1;
                    var endLine = startLine;
                    var startColumn = 1;
                    var endColumn = line.Length;

                    var violationExtent = new ScriptExtent(
                        new ScriptPosition(
                            ast.Extent.File,
                            startLine,
                            startColumn,
                            line
                        ),
                        new ScriptPosition(
                            ast.Extent.File,
                            endLine,
                            endColumn,
                            line
                        ));

                    var record = new DiagnosticRecord(
                            String.Format(CultureInfo.CurrentCulture, Strings.AvoidLongLinesError),
                            violationExtent,
                            GetName(),
                            GetDiagnosticSeverity(),
                            ast.Extent.File,
                            null
                        );
                    diagnosticRecords.Add(record);
                }
            }

            return diagnosticRecords;
        }

        /// <summary>
        /// Retrieves the common name of this rule.
        /// </summary>
        public override string GetCommonName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidLongLinesCommonName);
        }

        /// <summary>
        /// Retrieves the description of this rule.
        /// </summary>
        public override string GetDescription()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.AvoidLongLinesDescription);
        }

        /// <summary>
        /// Retrieves the name of this rule.
        /// </summary>
        public override string GetName()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                Strings.NameSpaceFormat,
                GetSourceName(),
                Strings.AvoidLongLinesName);
        }

        /// <summary>
        /// Retrieves the severity of the rule: error, warning or information.
        /// </summary>
        public override RuleSeverity GetSeverity()
        {
            return RuleSeverity.Warning;
        }

        /// <summary>
        /// Gets the severity of the returned diagnostic record: error, warning, or information.
        /// </summary>
        /// <returns></returns>
        public DiagnosticSeverity GetDiagnosticSeverity()
        {
            return DiagnosticSeverity.Warning;
        }

        /// <summary>
        /// Retrieves the name of the module/assembly the rule is from.
        /// </summary>
        public override string GetSourceName()
        {
            return string.Format(CultureInfo.CurrentCulture, Strings.SourceName);
        }

        /// <summary>
        /// Retrieves the type of the rule, Builtin, Managed or Module.
        /// </summary>
        public override SourceType GetSourceType()
        {
            return SourceType.Builtin;
        }
    }
}
