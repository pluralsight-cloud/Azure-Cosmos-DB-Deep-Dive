using ScintillaNET;
using System.Drawing;

namespace JsonSqlQuery
{
	public static class ScintillaExtensions
	{
		public static void ConfigureScintillaForXml(this Scintilla scintilla, string placeholder = "")
		{
			// Set placeholder
			scintilla.Text = placeholder; // e.g., "<Sample>\r\n\tData\r\n</Sample>";

			// Initialize common scintilla behavior
			scintilla.ConfigureScintillaForCommon(Lexer.Xml);

			// Set the Styles
			scintilla.StyleClearAll();
			scintilla.Styles[Style.Xml.Attribute].ForeColor = Color.Red;
			scintilla.Styles[Style.Xml.Tag].ForeColor = Color.FromArgb(163, 21, 21);
			scintilla.Styles[Style.Xml.DoubleString].ForeColor = Color.Blue;
		}

		public static void ConfigureScintillaForJson(this Scintilla scintilla, string placeholder = "")
		{
			// Set placeholder
			scintilla.Text = placeholder; // e.g., "{\r\n\t\"sample\": \"data\"\r\n}";

			// Initialize common scintilla behavior
			scintilla.ConfigureScintillaForCommon(Lexer.Json, marginWidth: 36);

			// Set the Styles
			scintilla.StyleClearAll();
			scintilla.Styles[Style.Json.PropertyName].ForeColor = Color.FromArgb(46, 117, 182);
			scintilla.Styles[Style.Json.String].ForeColor = Color.FromArgb(163, 21, 21);
		}

		public static void ConfigureScintillaForCSharp(this Scintilla scintilla)
		{
			// Set placeholder
			//scintilla.Text = "// C#";

			// Initialize common scintilla behavior
			scintilla.ConfigureScintillaForCommon(Lexer.Cpp);

			// Set the Styles
			scintilla.StyleClearAll();

			scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.Green;
			scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.Green;
			scintilla.Styles[Style.Cpp.CommentDoc].ForeColor = Color.Blue;
			scintilla.Styles[Style.Cpp.String].ForeColor = Color.DarkRed;
			scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Blue;

			scintilla.SetKeywords(0, "class extends implements import interface new case do while else if for in switch throw get set function var try catch finally while with default break continue delete return each const namespace package include use is as instanceof typeof author copy default deprecated eventType example exampleText exception haxe inheritDoc internal link mtasc mxmlc param private return see serial serialData serialField since throws usage version langversion playerversion productversion dynamic private public partial static intrinsic internal native override protected AS3 final super this arguments null Infinity NaN undefined true false abstract as base bool break by byte case catch char checked class const continue decimal default delegate do double descending explicit event extern else enum false finally fixed float for foreach from goto group if implicit in int interface internal into is lock long new null namespace object operator out override orderby params private protected public readonly ref return switch struct sbyte sealed short sizeof stackalloc static string select this throw true try typeof uint ulong unchecked unsafe ushort using var virtual volatile void while where yield");
			scintilla.SetKeywords(1, "void Null ArgumentError arguments Array Boolean Class Date DefinitionError Error EvalError Function int Math Namespace Number Object RangeError ReferenceError RegExp SecurityError String SyntaxError TypeError uint XML XMLList Boolean Byte Char DateTime Decimal Double Int16 Int32 Int64 IntPtr SByte Single UInt16 UInt32 UInt64 UIntPtr Void Path File System Windows Forms ScintillaNET");
		}

		public static void ConfigureScintillaForSql(this Scintilla scintilla, string placeholder = "")
		{
			// Set placeholder
			scintilla.Text = placeholder; // e.g., "SELECT * FROM sys.tables";

			// Initialize common scintilla behavior
			scintilla.ConfigureScintillaForCommon(Lexer.Sql);

			// Set the Styles
			scintilla.StyleClearAll();
			scintilla.Styles[Style.LineNumber].ForeColor = Color.FromArgb(255, 128, 128, 128);  //Dark Gray
			scintilla.Styles[Style.LineNumber].BackColor = Color.FromArgb(255, 228, 228, 228);  //Light Gray
			scintilla.Styles[Style.Sql.Comment].ForeColor = Color.Green;
			scintilla.Styles[Style.Sql.CommentLine].ForeColor = Color.Green;
			scintilla.Styles[Style.Sql.CommentLineDoc].ForeColor = Color.Green;
			scintilla.Styles[Style.Sql.Number].ForeColor = Color.Maroon;
			scintilla.Styles[Style.Sql.Word].ForeColor = Color.Blue;
			scintilla.Styles[Style.Sql.Word2].ForeColor = Color.Fuchsia;
			scintilla.Styles[Style.Sql.User1].ForeColor = Color.Gray;
			scintilla.Styles[Style.Sql.User2].ForeColor = Color.FromArgb(255, 00, 128, 192);    //Medium Blue-Green
			scintilla.Styles[Style.Sql.User3].ForeColor = Color.DarkRed;
			scintilla.Styles[Style.Sql.String].ForeColor = Color.Red;
			scintilla.Styles[Style.Sql.Character].ForeColor = Color.Red;
			scintilla.Styles[Style.Sql.Operator].ForeColor = Color.Black;

			// Set keyword lists
			// Word = 0 Blue
			scintilla.SetKeywords(0, @"add alter as authorization backup begin bigint binary bit break browse bulk by cascade case catch check checkpoint close clustered column commit compute constraint containstable continue create current cursor cursor database date datetime datetime2 datetimeoffset dbcc deallocate decimal declare default delete deny desc disk distinct distributed double drop dump else end errlvl escape except exec execute exit external fetch file fillfactor float for foreign freetext freetexttable from full function goto grant group having hierarchyid holdlock identity identity_insert identitycol if image index insert int intersect into key kill lineno load merge money national nchar nocheck nocount nolock nonclustered ntext numeric nvarchar of off offsets on open opendatasource openquery openrowset openxml option order over percent plan precision primary print proc procedure public raiserror read readtext real reconfigure references replication restore restrict return revert revoke rollback rowguidcol rule save schema securityaudit select set setuser shutdown smalldatetime smallint smallmoney sql_variant statistics table table tablesample text textsize then time timestamp tinyint to top tran transaction trigger truncate try union unique uniqueidentifier update updatetext use user values varbinary varchar varying view waitfor when where while with writetext xml go single_user immediate masked password security policy filter predicate role block after generated always row start period system_time system_versioning master encryption remote_data_archive server credential scoped json auto without_array_wrapper include_null_values root path openjson filegroup filestream single_blob returns schemabinding ");
			// Word2 = 1 Fuchsia
			scintilla.SetKeywords(1, @"st_distance st_intersects st_isvalid st_isvaliddetailed value min max avg sum left right length startswith endswith index_of ltrim rtrim lower upper regexmatch stringtoarray stringtoobject stringtoboolean stringtonull stringtonumber tostring datetimeadd datetimebin datetimediff datetimefromparts datetimepart datetimetoticks datetimetotimestamp tickstodatetime timestamptodatetime getcurrentdatetime getcurrentticks getcurrenttimestamp array_concat array_contains array_length array_slice is_array is_bool is_null is_object round trunc abs sin asin cos acos tan atan atn2 cot degrees exp log log10 power radians sign square sqrt pi ascii cast char charindex ceiling coalesce collate contains convert current_date current_time current_timestamp current_user floor isnull nullif object_id session_user substring system_user tsequal count datefromparts sysutcdatetime dateadd concat switchoffset permission_name sysdatetime database_principal_id user_name rowcount session_context rand isjson json_value json_query newsequentialid datalength db_name filetablerootpath getpathlocator ");
			// User1 = 4 Blue
			scintilla.SetKeywords(4, @"all and any between cross exists in inner is join like not null or outer pivot some unpivot ( ) * ");
			// User2 = 5 Medium Blue-Green
			scintilla.SetKeywords(5, @"sys objects sysobjects masked_columns databases tables columns database_permissions sysusers syslogins column_master_keys column_encryption_keys column_encryption_key_values remote_data_archive_databases filegroups database_filestream_options ");
			// User3 = 6 DarkRed
			scintilla.SetKeywords(6, @"sp_configure sp_addrolemember sp_set_session_context sp_spaceused sp_filestream_force_garbage_collection ");

		}

		public static void ConfigureScintillaForCommon(this Scintilla scintilla, Lexer lexer, int marginWidth = 28)
		{
			// https://gist.github.com/anonymous/63036aa8c1cefcfcb013

			// Reset the styles
			scintilla.StyleResetDefault();
			scintilla.Styles[Style.Default].Font = "Cascadia Mono";
			scintilla.Styles[Style.Default].Size = 10;
			scintilla.StyleClearAll();

			// Set the lexer
			scintilla.Lexer = lexer;

			// Show line numbers
			scintilla.Margins[0].Width = marginWidth;

			// Enable folding
			scintilla.SetProperty("fold", "1");
			scintilla.SetProperty("fold.compact", "1");
			scintilla.SetProperty("fold.html", "1");

			// Use Margin 2 for fold markers
			scintilla.Margins[2].Type = MarginType.Symbol;
			scintilla.Margins[2].Mask = Marker.MaskFolders;
			scintilla.Margins[2].Sensitive = true;
			scintilla.Margins[2].Width = 20;

			// Reset folder markers
			for (int i = Marker.FolderEnd; i <= Marker.FolderOpen; i++)
			{
				scintilla.Markers[i].SetForeColor(SystemColors.ControlLightLight);
				scintilla.Markers[i].SetBackColor(SystemColors.ControlDark);
			}

			// Style the folder markers
			scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
			scintilla.Markers[Marker.Folder].SetBackColor(SystemColors.ControlText);
			scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
			scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
			scintilla.Markers[Marker.FolderEnd].SetBackColor(SystemColors.ControlText);
			scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
			scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
			scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
			scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

			// Enable automatic folding
			scintilla.AutomaticFold = AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change;
		}

	}
}
