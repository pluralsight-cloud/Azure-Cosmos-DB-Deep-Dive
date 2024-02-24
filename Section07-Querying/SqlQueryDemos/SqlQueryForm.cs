using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JsonSqlQuery
{
	public partial class SqlQueryForm : Form
	{
		private CosmosClient _client;
		private JToken _results;
		private int _resultCount;

		public SqlQueryForm()
		{
			InitializeComponent();
		}

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var endpoint = ConfigurationManager.AppSettings["Endpoint"];
			var masterKey = ConfigurationManager.AppSettings["MasterKey"];
			this._client = new CosmosClient(endpoint, masterKey);

			this.SqlTextBox.ConfigureScintillaForSql();
			this.JsonTextBox.ConfigureScintillaForJson();

			this.ContainerToolStripComboBox.Items.Clear();
			this.ContainerToolStripComboBox.Items.Add("Families");
			this.ContainerToolStripComboBox.Items.Add("Stores");

			this.SqlTextBox.Text = File.ReadAllText(@"..\..\..\SqlQueryDemos.txt");

			this.ShowHideResultsGrid(false);
		}

		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (e.KeyCode == Keys.F5 && e.Modifiers == Keys.None)
			{
				this.Execute();
				e.Handled = true;
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			base.OnKeyPress(e);

			// CTRL+R
			if (e.KeyChar == (char)18)
			{
				this.ShowHideResultsGrid(this.MainSplitContainer.Panel2Collapsed);
				e.Handled = true;
			}

			// CTRL+A
			if (e.KeyChar == (char)1)
			{
				this.SqlTextBox.SelectAll();
				e.Handled = true;
			}
		}

		private void ContainerToolStripComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			this.SqlTextBox.Focus();
		}

		private void ExecuteToolStripButton_Click(object sender, EventArgs e)
		{
			this.Execute();
		}

		private async void Execute()
		{
			var sql = this.SqlTextBox.SelectedText;
			if (string.IsNullOrWhiteSpace(sql))
			{
				sql = this.SqlTextBox.Text;
			}

			this.ResultsInfoLabel.Text = "Executing query...";
			this.JsonTextBox.ReadOnly = false;
			this.JsonTextBox.Text = string.Empty;
			this.JsonTextBox.ReadOnly = true;
			this.ShowHideResultsGrid(true);

			try
			{
				await this.RunSql(sql);
			}
			catch (Exception ex)
			{
				this.ResultsInfoLabel.Text = "Error";
				MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			var json = JsonConvert.SerializeObject(this._results);
			var formattedJson = this.FormatJson(json);

			this.JsonTextBox.ReadOnly = false;
			this.JsonTextBox.Text = formattedJson;
			this.JsonTextBox.ReadOnly = true;

			this.ResultsInfoLabel.Text = $"Returned {this._resultCount} document(s)";

			this.SqlTextBox.Focus();
		}

		private async Task RunSql(string sql)
		{
			var databaseName = default(string);
			var containerName = default(string);

			switch (this.ContainerToolStripComboBox.Text)
			{
				case "Families":
					databaseName = "Families";
					containerName = "Families";
					break;

				case "Stores":
					databaseName = "adventure-works";
					containerName = "stores";
					break;
			}

			var container = this._client.GetContainer(databaseName, containerName);
			var iterator = container.GetItemQueryIterator<JToken>(sql);
			var itemCount = 0;
			var pageCount = 0;
			var results = new JArray();
			while (iterator.HasMoreResults)
			{
				pageCount++;
				var documents = await iterator.ReadNextAsync();
				foreach (var document in documents)
				{
					itemCount++;
					results.Add(document);
				}
			}

			this._results = results;
			this._resultCount = itemCount;
		}

        private string FormatJson(string unformattedJson)
		{
			const string Indent = "  ";

			unformattedJson = unformattedJson
				.Replace(Environment.NewLine, " ")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;")
			;

			var level = 0;
			var quoteCount = 0;
			var result =
				from ch in unformattedJson
				let quotes = ch == '"' ? quoteCount++ : quoteCount
				let lineBreak = ch == ',' && quotes % 2 == 0 ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, level)) : null
				let openChar = ch == '{' || ch == '[' ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, ++level)) : ch.ToString()
				let closeChar = ch == '}' || ch == ']' ? Environment.NewLine + string.Concat(Enumerable.Repeat(Indent, --level)) + ch : ch.ToString()
				select lineBreak ?? (openChar.Length > 1 ? openChar : closeChar)
			;

			var json = string.Concat(result);
			return json;
		}

		private void ShowHideResultsGrid(bool show)
		{
			this.MainSplitContainer.Panel2Collapsed = !show;
			this.SqlTextBox.Focus();
		}

	}
}
