using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Neo4jClient;
using NBP_n4j_v1.DomainModel;
using Neo4jClient.Cypher;

namespace NBP_n4j_v1
{
    public partial class RegisterForm : Form
    {
        private GraphClient client;
        public User registeredUser;
        public RegisterForm()
        {
            InitializeComponent();
        }
        public RegisterForm(GraphClient client)
        {
            InitializeComponent();
            this.client = client;
        }

        private void btnSignIn_Click(object sender, EventArgs e)
        {

            List<User> usersWithEmail = this.client.Cypher.Match("(user:User { email: '" + this.txtEmail.Text + "' })")
                                            .Return<User>("user")
                                            .Results
                                            .ToList();
            if (usersWithEmail.Count > 0){
                MessageBox.Show("Email alredy exists!");
                return;
            }

            this.client.Cypher.Create("(user:User { name: '" + this.txtName.Text + "', email: '" + this.txtEmail.Text + "', password: '" + this.txtPassword.Text + "'})")
                                .Merge("(loc:Location { name: '" + this.txtLocation.Text + "'})")
                                .Merge("(user)-[:LIVES_IN]->(loc)").ExecuteWithoutResults();
            this.registeredUser = new User(this.txtName.Text, this.txtEmail.Text, this.txtPassword.Text);
            MessageBox.Show("Register successfull");
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }
    }
}
