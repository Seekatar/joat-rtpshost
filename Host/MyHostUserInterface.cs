// <copyright file="MyHostUserInterface.cs" company="Microsoft Corporation">
// Copyright (c) 2009 Microsoft Corporation. All rights reserved.
// </copyright>
// DISCLAIMER OF WARRANTY: The software is licensed “as-is.” You 
// bear the risk of using it. Microsoft gives no express warranties, 
// guarantees or conditions. You may have additional consumer rights 
// under your local laws which this agreement cannot change. To the extent 
// permitted under your local laws, Microsoft excludes the implied warranties 
// of merchantability, fitness for a particular purpose and non-infringement.

namespace RtPsHost
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Text;

    /// <summary>
    /// Sample implementation of the PSHostUserInterface class.
    /// This is more complete than the previous samples however
    /// there are still members that are unimplemented by this class.
    /// Members that map easily onto the existing .NET console APIs are 
    /// supported. Members that are not implemented usually throw a
    /// NotImplementedException exception though some just do nothing 
    /// and silently return. Also, a basic implementation of the prompt
    /// API is provided. The credential and secure string methods are
    /// not supported.
    /// </summary>
    internal class MyHostUserInterface : PSHostUserInterface, IHostUISupportsMultipleChoiceSelection, IDisposable
    {
        private IPsConsole program;
        private MyRawUserInterface myRawUi;

        public MyHostUserInterface(IPsConsole program)
        {
            this.program = program;
            myRawUi = new MyRawUserInterface(program);
        }

        /// <summary>
        /// Gets an instance of the PSRawUserInterface object for this host
        /// application.
        /// </summary>
        public override PSHostRawUserInterface RawUI
        {
            get { return this.myRawUi; }
        }

        /// <summary>
        /// Prompts the user for input.
        /// </summary>
        /// <param name="caption">Text that preceeds the prompt (a title).</param>
        /// <param name="message">Text of the prompt.</param>
        /// <param name="descriptions">A collection of FieldDescription objects 
        /// that contains the user input.</param>
        /// <returns>A dictionary object that contains the results of the user prompts.</returns>
        public override Dictionary<string, PSObject> Prompt(
                       string caption,
                       string message,
                       Collection<FieldDescription> descriptions)
        {
            // Read-Host gets here
            if (descriptions.Count == 0)
                throw new NotImplementedException("Must have descriptions");

            var desc =  descriptions.First();

            if (desc.ParameterTypeFullName == typeof(System.Security.SecureString).FullName)
            {
                var promptRet = program.PromptForSecureString(caption, message, desc.Name);
                return new Dictionary<string, PSObject>() { { desc.Name, new PSObject(promptRet ?? new System.Security.SecureString()) } };
            }
            else if (desc.ParameterTypeFullName == typeof(String).FullName)
            {
                string promptRet = program.PromptForString(caption, message, desc.Name);
                return new Dictionary<string, PSObject>() { { desc.Name, new PSObject(promptRet ?? String.Empty) } };
            }
            else
                throw new NotImplementedException("Unsupported type of " + desc.ParameterTypeFullName);
        }

        /// <summary>
        /// Provides a set of choices that enable the user to choose a single option 
        /// from a set of options. 
        /// </summary>
        /// <param name="caption">A title that proceeds the choices.</param>
        /// <param name="message">An introduction  message that describes the 
        /// choices.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that describ 
        /// each choice.</param>
        /// <param name="defaultChoice">The index of the label in the Choices parameter 
        /// collection that indicates the default choice used if the user does not specify 
        /// a choice. To indicate no default choice, set to -1.</param>
        /// <returns>The index of the Choices parameter collection element that corresponds 
        /// to the option that is selected by the user.</returns>
        public override int PromptForChoice(
                                            string caption,
                                            string message,
                                            Collection<ChoiceDescription> choices,
                                            int defaultChoice)
        {
            return program.PromptForChoice(caption, message, choices.Select(o => new PromptChoice(o.Label, o.HelpMessage)), defaultChoice);
        }

        #region IHostUISupportsMultipleChoiceSelection Members
        /// <summary>
        /// Provides a set of choices that enable the user to choose a one or more options 
        /// from a set of options. 
        /// </summary>
        /// <param name="caption">A title that proceeds the choices.</param>
        /// <param name="message">An introduction  message that describes the 
        /// choices.</param>
        /// <param name="choices">A collection of ChoiceDescription objects that describe each choice.</param>
        /// <param name="defaultChoices">The index of the label in the Choices parameter 
        /// collection that indicates the default choice used if the user does not specify 
        /// a choice. To indicate no default choice, set to -1.</param>
        /// <returns>The index of the Choices parameter collection element that corresponds 
        /// to the choices selected by the user.</returns>
        public Collection<int> PromptForChoice(
                                               string caption,
                                               string message,
                                               Collection<ChoiceDescription> choices,
                                               IEnumerable<int> defaultChoices)
        {
            throw new NotImplementedException("The method PromptForChoice() with multiple defaultChoices is not implemented by MyHost.");
        }

        #endregion

        /// <summary>
        /// Prompts the user for credentials with a specified prompt window 
        /// caption, prompt message, user name, and target name.
        /// </summary>
        /// <param name="caption">The caption of the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be prompted for.</param>
        /// <param name="targetName">The name of the target for which the credential is collected.</param>
        /// <returns>Throws a NotImplementException exception.</returns>
        public override PSCredential PromptForCredential(
            string caption, string message, string userName, string targetName)
        {
            throw new NotImplementedException("The method PromptForCredential() is not implemented by MyHost.");
        }

        /// <summary>
        /// Prompts the user for credentials by using a specified prompt window 
        /// caption, prompt message, user name and target name, credential types 
        /// allowed to be returned, and UI behavior options.
        /// </summary>
        /// <param name="caption">The caption of the message window.</param>
        /// <param name="message">The text of the message.</param>
        /// <param name="userName">The user name whose credential is to be prompted for.</param>
        /// <param name="targetName">The name of the target for which the credential is collected.</param>
        /// <param name="allowedCredentialTypes">PSCredentialTypes cconstants that identify the type of 
        /// credentials that can be returned.</param>
        /// <param name="options">A PSCredentialUIOptions constant that identifies the UI behavior 
        /// when it gathers the credentials.</param>
        /// <returns>Throws a NotImplementException exception.</returns>
        public override PSCredential PromptForCredential(
                               string caption,
                               string message,
                               string userName,
                               string targetName,
                               PSCredentialTypes allowedCredentialTypes,
                               PSCredentialUIOptions options)
        {
            throw new NotImplementedException("The method PromptForCredential() is not implemented by MyHost.");
        }

        /// <summary>
        /// Reads characters that are entered by the user until a 
        /// newline (carriage return) is encountered.
        /// </summary>
        /// <returns>The characters entered by the user.</returns>
        public override string ReadLine()
        {
            throw new NotImplementedException("The method ReadLine() is not implemented by MyHost.");
        }

        /// Reads characters entered by the user until a newline (carriage return) 
        /// is encountered and returns the characters as a secure string.
        /// </summary>
        /// <returns>A secure string of the characters entered by the user.</returns>
        public override System.Security.SecureString ReadLineAsSecureString()
        {
            throw new NotImplementedException("The method ReadLineAsSecureString() is not implemented by MyHost.");
        }

        /// <summary>
        /// Writes a line of characters to the output display of the host 
        /// and appends a newline (carriage return).
        /// </summary>
        /// <param name="value">The characters to be written.</param>
        public override void Write(string value)
        {
            program.Write(value, WriteType.Output);
        }

        /// <summary>
        /// Writes characters to the output display of the host with possible 
        /// foreground and background colors. 
        /// </summary>
        /// <param name="foregroundColor">The color of the characters.</param>
        /// <param name="backgroundColor">The backgound color to use.</param>
        /// <param name="value">The characters to be written.</param>
        public override void Write(
                                   ConsoleColor foregroundColor,
                                   ConsoleColor backgroundColor,
                                   string value)
        {
            program.Write(foregroundColor, backgroundColor, value, WriteType.Host);
        }

        /// <summary>
        /// Writes a line of characters to the output display of the host 
        /// with foreground and background colors and appends a newline (carriage return). 
        /// </summary>
        /// <param name="foregroundColor">The forground color of the display. </param>
        /// <param name="backgroundColor">The background color of the display. </param>
        /// <param name="value">The line to be written.</param>
        public override void WriteLine(
                                       ConsoleColor foregroundColor,
                                       ConsoleColor backgroundColor,
                                       string value)
        {
            program.WriteLine(foregroundColor, backgroundColor, value, WriteType.Host);
        }

        /// <summary>
        /// Writes a debug message to the output display of the host.
        /// </summary>
        /// <param name="message">The debug message that is displayed.</param>
        public override void WriteDebugLine(string message)
        {
            program.WriteLine(ConsoleColor.DarkCyan,
                           ConsoleColor.Black,
                           String.Format(CultureInfo.CurrentCulture, "DEBUG: {0}", message), WriteType.Debug);
        }

        /// <summary>
        /// Writes an error message to the output display of the host.
        /// </summary>
        /// <param name="value">The error message that is displayed.</param>
        public override void WriteErrorLine(string value)
        {
            program.WriteLine(ConsoleColor.Red, ConsoleColor.Black, value, WriteType.Error);
        }

        /// <summary>
        /// Writes a newline character (carriage return) 
        /// to the output display of the host. 
        /// </summary>
        public override void WriteLine()
        {
            program.WriteLine(String.Empty, WriteType.Host);
        }

        /// <summary>
        /// Writes a line of characters to the output display of the host 
        /// and appends a newline character(carriage return). 
        /// </summary>
        /// <param name="value">The line to be written.</param>
        public override void WriteLine(string value)
        {
            program.WriteLine(value, WriteType.Host);
        }

        /// <summary>
        /// Writes a verbose message to the output display of the host.
        /// </summary>
        /// <param name="message">The verbose message that is displayed.</param>
        public override void WriteVerboseLine(string message)
        {
            program.WriteLine(
                           ConsoleColor.Yellow,
                           ConsoleColor.Black,
                           String.Format(CultureInfo.CurrentCulture, "VERBOSE: {0}", message), WriteType.Verbose);
        }

        /// <summary>
        /// Writes a warning message to the output display of the host.
        /// </summary>
        /// <param name="message">The warning message that is displayed.</param>
        public override void WriteWarningLine(string message)
        {
            program.WriteLine(
                           ConsoleColor.Yellow,
                           ConsoleColor.Black,
                           String.Format(CultureInfo.CurrentCulture, "WARNING: {0}", message), WriteType.Warning);
        }

        /// <summary>
        /// Writes a progress report to the output display of the host. 
        /// Writing a progress report is not required for the cmdlet to 
        /// work so it is better to do nothing instead of throwing an 
        /// exception.
        /// </summary>
        /// <param name="sourceId">Unique identifier of the source of the record. </param>
        /// <param name="record">A ProgressReport object.</param>
        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            program.WriteProgress(new ProgressInfo(record.Activity, record.StatusDescription, record.CurrentOperation, record.RecordType == ProgressRecordType.Completed ? 100 : record.PercentComplete, record.ActivityId, record.SecondsRemaining));
        }

        public void Dispose()
        {
            this.program = null;
            if (myRawUi != null)
            {
                this.myRawUi.Dispose();
            }
        }
    }
}

