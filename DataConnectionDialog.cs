using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Security.Principal;

namespace com.rusanu.dataconnectiondialog
{
    /// <summary>
    /// SQL Server connection dialog
    /// </summary>
    public partial class DataConnectionDialog : Form
    {
        /// <summary>
        /// The connection string builder
        /// </summary>
        public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }

        /// <summary>
        /// Internal class for form/connection properties
        /// </summary>
        internal class Properties : INotifyPropertyChanged
        {
            /// <summary>
            /// Strings used in code
            /// </summary>
            internal class Tags
            {
                public static readonly string DataSource = "DataSource";
                public static readonly string UserName = "UserName";
                public static readonly string Password = "Password";
                public static readonly string IntegratedSecurity = "IntegratedSecurity";
                public static readonly string DataSourceValid = "DataSourceValid";
                public static readonly string UserNameEnabled = "UserNameEnabled";
                public static readonly string WindowsAuthentication = "Windows Authentication";
                public static readonly string SQLServerAuthentication = "SQL Server Authentication";
                public static readonly string AuthenticationMode = "AuthenticationMode";
                public static readonly string TestingEnabled = "TestingEnabled";
                public static readonly string TestResult = "TestResult";
            };

            /// <summary>
            /// From INotifyPropertyChanged
            /// </summary>
            public event PropertyChangedEventHandler PropertyChanged;

            /// <summary>
            /// The current connection string builder
            /// </summary>
            public SqlConnectionStringBuilder ConnectionStringBuilder { get; private set; }

            /// <summary>
            /// ConnectionStreingBuilder.DataSource + change notifications
            /// </summary>
            public string DataSource
            {
                get { return ConnectionStringBuilder.DataSource; }
                set
                {
                    ConnectionStringBuilder.DataSource = value;
                    NotifyPropertyChanged(Tags.DataSource);
                    NotifyPropertyChanged(Tags.DataSourceValid);
                    NotifyPropertyChanged(Tags.TestingEnabled);
                    TestResult = String.Empty;
                }
            }

            /// <summary>
            /// True when the data source is not empty (for Enabled data binding)
            /// </summary>
            public bool DataSourceValid
            {
                get { return !string.IsNullOrEmpty(DataSource); }
            }

            /// <summary>
            /// ConnectionStringBuilder.IntegratedSecurity + change notifications
            /// </summary>
            public bool IntegratedSecurity
            {
                get { return ConnectionStringBuilder.IntegratedSecurity; }
                set
                {
                    if (value != ConnectionStringBuilder.IntegratedSecurity)
                    {
                        ConnectionStringBuilder.IntegratedSecurity = value;
                        NotifyPropertyChanged(Tags.IntegratedSecurity);
                        NotifyPropertyChanged(Tags.UserNameEnabled);
                        NotifyPropertyChanged(Tags.AuthenticationMode);
                        TestResult = String.Empty;
                    }
                }
            }

            /// <summary>
            /// ConnectionStringBuilder.UserID + change notifications
            /// For IntegratedSecurity it returns current Windows identity
            /// </summary>
            public string UserName
            {
                get { return IntegratedSecurity ? WindowsUserName : ConnectionStringBuilder.UserID; }
                set
                {
                    ConnectionStringBuilder.UserID = value;
                    NotifyPropertyChanged(Tags.UserName);
                    TestResult = String.Empty;
                }
            }

            /// <summary>
            /// ConnectionStringBuilder.Password + change notifications
            /// For IntegratedSecurity will return ""
            /// </summary>
            public string Password
            {
                get { return IntegratedSecurity ? String.Empty : ConnectionStringBuilder.Password; }
                set
                {
                    ConnectionStringBuilder.Password = value;
                    NotifyPropertyChanged(Tags.Password);
                    TestResult = String.Empty;
                }
            }

            /// <summary>
            /// True when not ConnectionStringBuilder.IntegratedSecurity (for Enabled binding)
            /// </summary>
            public bool UserNameEnabled
            {
                get { return !IntegratedSecurity; }
            }

            /// <summary>
            /// ConnectionStringBuilder.IntegratedSecurity mapped to values for the combo data binding
            /// </summary>
            public string AuthenticationMode
            {
                get { return IntegratedSecurity ? Tags.WindowsAuthentication : Tags.SQLServerAuthentication; }
                set
                {
                    IntegratedSecurity = (value == Tags.WindowsAuthentication);
                }
            }

            private bool _isTesting = false;

            /// <summary>
            /// Set to true when the connection is being tested
            /// </summary>
            public bool IsTesting 
            { 
                get { return _isTesting; }
                set
                {
                    _isTesting = value;
                    NotifyPropertyChanged(Tags.TestingEnabled);
                }
            }

            /// <summary>
            /// True when Testing is allowed (for Rnabled binding)
            /// </summary>
            public bool TestingEnabled
            {
                get { return DataSourceValid && !IsTesting; }
            }

            private string _testResult = "";

            /// <summary>
            /// Connection test result
            /// </summary>
            public string TestResult
            {
                get {return _testResult;}
                set
                {
                    _testResult = value;
                    NotifyPropertyChanged(Tags.TestResult);
                }
            }

            /// <summary>
            /// The current Windows user, cached
            /// </summary>
            internal string WindowsUserName { get; set; }

            /// <summary>
            /// Notification for INotifyPropertyChanged
            /// </summary>
            /// <param name="propertyName"></param>
            internal void NotifyPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            /// <summary>
            /// CTOR
            /// </summary>
            /// <param name="scsb">Preconfigured SqlConnectionStringBuilder</param>
            public Properties(SqlConnectionStringBuilder scsb)
            {
                ConnectionStringBuilder = scsb;
                WindowsUserName = WindowsIdentity.GetCurrent().Name;
            }
        }

        /// <summary>
        /// The current properties
        /// </summary>
        private Properties FormProperties { get; set; }

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="connectionStringBuilder">The preconfigured connection string builder</param>
        public DataConnectionDialog(SqlConnectionStringBuilder connectionStringBuilder)
        {
            InitializeComponent();

            authenticationMode.Items.Add(Properties.Tags.WindowsAuthentication);
            authenticationMode.Items.Add(Properties.Tags.SQLServerAuthentication);

            ConnectionStringBuilder = connectionStringBuilder ?? new SqlConnectionStringBuilder()
                {
                    IntegratedSecurity = true,
                    DataSource = "."
                };

            FormProperties = new Properties(ConnectionStringBuilder);

            serverName.DataBindings.Add("Text", FormProperties, Properties.Tags.DataSource, false, DataSourceUpdateMode.OnPropertyChanged);
            userName.DataBindings.Add("Text", FormProperties, Properties.Tags.UserName);
            userPassword.DataBindings.Add("Text", FormProperties, Properties.Tags.Password);

            authenticationMode.DataBindings.Add("Text", FormProperties, Properties.Tags.AuthenticationMode, false, DataSourceUpdateMode.OnPropertyChanged);

            userName.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);
            userPassword.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);
            lblUserName.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);
            lblPassword.DataBindings.Add("Enabled", FormProperties, Properties.Tags.UserNameEnabled);

            btnConnect.DataBindings.Add("Enabled", FormProperties, Properties.Tags.DataSourceValid);
            btnTest.DataBindings.Add("Enabled", FormProperties, Properties.Tags.TestingEnabled);

            lblStatus.DataBindings.Add("Text", FormProperties, Properties.Tags.TestResult);
        }

        internal void TestConnection()
        {
            using (SqlConnection conn = new SqlConnection(ConnectionStringBuilder.ConnectionString))
            {
                try
                {
                    FormProperties.IsTesting = true;
                    FormProperties.TestResult = "Testing...";
                    conn.Open();
                    FormProperties.TestResult = "Success!";
                }
                catch (Exception ex)
                {
                    FormProperties.TestResult = ex.Message;
                }
                finally
                {
                    FormProperties.IsTesting = false;
                }
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            TestConnection();
        }

        
    }
}
