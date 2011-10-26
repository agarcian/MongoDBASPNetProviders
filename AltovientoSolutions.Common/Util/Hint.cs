using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.Common.Util
{
    /// <summary>
    /// Explicitely created to support JQueryUI Autocomplete, 
    /// to generically handle results with the following properties: value, label, descr.
    /// Other properties can be added to add custom display.
    /// </summary>
    public class Hint
    {
        string _value = null;

        public string value
        {
            get { return _value; }
            set { _value = value; }
        }
        string _label = null;

        public string label
        {
            get { return _label; }
            set { _label = value; }
        }
        string _desc = null;

        public string desc
        {
            get { return _desc; }
            set { _desc = value; }
        }

        public Hint()
        {
        }

        public Hint(string Value, string Label)
        {
            this._value = Value;
            this._label = Label;
        }

        public Hint(string Value, string Label, string Description)
        {
            this._value = Value;
            this._label = Label;
            this._desc = Description;
        }

    }
}
