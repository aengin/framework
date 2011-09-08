﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Signum.Utilities.Reflection;
using System.Collections;
using System.Reflection;
using Signum.Utilities;
using Signum.Entities.Properties;

namespace Signum.Entities.DynamicQuery
{
    [Serializable]
    public class ResultColumn
    {
        public ResultColumn(Column column, Array values)
        {
            this.Column = column;
            this.Values = values;
        }
        public Column Column { get; private set; }
        public int Index { get; internal set; }
        internal Array Values;
    }

    [Serializable]
    public class ResultTable
    {
        public bool HasEntities
        {
            get { return entityValues != null; }
        }
        
        internal Array entityValues;
        public ColumnDescription EntityColumn { get; private set; }
        public ResultColumn[] Columns { get; private set; }
         
        public ResultRow[] Rows { get; private set; }

        public ResultTable(params ResultColumn[] columns)
        {
            int rows = columns.Select(a => a.Values.Length).Distinct().Single("Unsyncronized number of rows in the results");

            ResultColumn entityColumn = columns.Where(c => c.Column is _EntityColumn).SingleOrDefault(); ;
            if (entityColumn != null)
            {
                this.EntityColumn = ((ColumnToken)entityColumn.Column.Token).Column;
                entityValues = entityColumn.Values; 
            }
            this.Columns = columns.Where(c => !(c.Column is _EntityColumn) && c.Column.Token.IsAllowed()).ToArray();

            for (int i = 0; i < Columns.Length; i++)
                Columns[i].Index = i;

            this.Rows = 0.To(rows).Select(i => new ResultRow(i, this)).ToArray();
        }


        public DataTable ToDataTable()
        {
            DataTable dt = new DataTable("Table");
            dt.Columns.AddRange(Columns.Select(c => new DataColumn(c.Column.Name, c.Column.Type)).ToArray());
            foreach (var row in Rows)
            {
                dt.Rows.Add(Columns.Select((_, i) => row[i]).ToArray());
            }
            return dt;
        }
    }


    [Serializable]
    public class ResultRow
    {
        public readonly int Index;
        public readonly ResultTable Table;

        public object this[int columnIndex]
        {
            get { return Table.Columns[columnIndex].Values.GetValue(Index); }
        }

        public object this[ResultColumn column]
        {
            get { return column.Values.GetValue(Index); }
        }

        internal ResultRow(int index, ResultTable table)
        {
            this.Index = index;
            this.Table = table;
        }

        public Lite Entity
        {
            get { return  (Lite)Table.entityValues.GetValue(Index); }
        }

        public T GetValue<T>(string columnName)
        {
            return (T)this[Table.Columns.Where(c => c.Column.Name == columnName).Single("column not found")];
        }

        public T GetValue<T>(int columnIndex)
        {
            return (T)this[columnIndex];
        }

        public T GetValue<T>(ResultColumn column)
        {
            return (T)this[column];
        }
    }
}
