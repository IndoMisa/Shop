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
using NBP_n4j_v1.HelperClasses;
namespace NBP_n4j_v1
{
    public partial class ClientForm : Form
    {
        private String email, password;
        private GraphClient client;
        private List<ProductType> typeList;
        private ProductWStorePriceAndQuantity chosenProduct;
        private List<ProductWithStoresAndPrices> productsToShow;
        private List<ProductWStorePriceAndQuantity> productsForCart;
        public ClientForm()
        {
            InitializeComponent();
        }
        public ClientForm(GraphClient client, String email, String password)
        {
            InitializeComponent();

            this.client = client;
            this.email = email;
            this.password = password;

            typeList = new List<ProductType>();
        }
        private void InitializeTypeList()
        {
            this.typeList = this.client.Cypher.Match("(t:ProductType)")
                                              .Return<ProductType>("t")
                                              .Results
                                              .ToList();
            LoadTypes();
        }
        private void LoadTypes()
        {
            CheckBox newCheckBox;
            foreach (var type in typeList)
            {
                newCheckBox = new CheckBox();
                newCheckBox.Name = "cb" + type.name;
                newCheckBox.Text = type.name;
                this.flpTypes.Controls.Add(newCheckBox);
            }
        }
        private bool ValidateEntry()
        {
            txtCompany.Text = txtCompany.Text.Trim(' ');
            txtLocation.Text = txtLocation.Text.Trim(' ');
            txtStore.Text = txtStore.Text.Trim(' ');
            if (numLower.Value > numUpper.Value)
            {
                MessageBox.Show("Upper price boundary must be greater or equal to the lower");
                return false;
            }
            return true;
        }
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (!ValidateEntry())
                return;
            txtSearch.Text = txtSearch.Text.Trim(' ');
            if (txtSearch.Text == "")
                return;
            String entry = this.txtSearch.Text;
            List<String> selectedTypes = GetTypesForFiltering();

            String additionalFilterQuery = "";
            if (txtLocation.Text.Length > 0)
            {
                additionalFilterQuery += "AND loc.name = '" + this.txtLocation.Text + "' ";
            }
            if (txtCompany.Text.Length > 0)
            {
                additionalFilterQuery += "AND company.name = '" + this.txtCompany.Text + "' ";
            }
            if (txtStore.Text.Length > 0)
            {
                additionalFilterQuery += "AND length(split(toLower(store.name), '" + txtStore.Text.ToLower() + "')[0]) < length(store.name)";
            }
            List<ProductWithStoresAndPrices> filteredList = this.client.Cypher.Match("(type:ProductType)<-[:BELONGS_TO]-(product:Product)<-[rel:SELLS]-(store:Store)-[:LOCATED_AT]->(loc:Location)")
                                                           .Match("(company:Company)-[:MAKES]->(product)")
                                                           .Where("type.name IN ['" + string.Join("','", selectedTypes) + "'] " + additionalFilterQuery)
                                                           .AndWhere("length(split(toLower(product.name), '" + entry.ToLower() + "')[0]) < length(product.name)")
                                                           .AndWhere("rel.price >= " + this.numLower.Value.ToString() + " and rel.price <= " + this.numUpper.Value.ToString())
                                                           .With("product, collect({ store: store, price: rel.price, link: rel.link}) as storesWithPrices")
                                                           .With("{ product: product, storesWithPricesAndLinks: storesWithPrices } as res")
                                                           .Return<ProductWithStoresAndPrices>("res")
                                                           .Results
                                                           .ToList();
            productsToShow = filteredList;
            ShowProducts(filteredList);
        }
        private void ShowProducts(List<ProductWithStoresAndPrices> productList)
        {
            ClearSearch();
            for (int i = 0; i < productList.Count; i++)
            {
                FlowLayoutPanel newProductPanel = new FlowLayoutPanel();
                newProductPanel.Name = "flp" + i.ToString();
                newProductPanel.Width = flpProductView.Width;
                newProductPanel.Height = 100;
                newProductPanel.FlowDirection = FlowDirection.TopDown;
                newProductPanel.Controls.Add(new Label() { Height = 1, Width = newProductPanel.Width, Dock = DockStyle.Bottom, BackColor = Color.DimGray });
                this.flpProductView.Controls.Add(newProductPanel);

                Label lblName = new Label();
                lblName.Text = "Name: " + productList[i].product.name;
                lblName.Width = lblName.Text.Length * 10;
                newProductPanel.Controls.Add(lblName);

                float minPrice = 1000000, maxPrice = 0;
                foreach (StoreWithPriceAndLink store in productList[i].storesWithPricesAndLinks)
                {
                    if (store.price < minPrice)
                        minPrice = store.price;
                    if (store.price > maxPrice)
                        maxPrice = store.price;
                }

                Label lblPrice = new Label();
                if (minPrice == maxPrice)
                    lblPrice.Text = "Price: " + minPrice.ToString();
                else
                    lblPrice.Text = "Price: " + minPrice.ToString() + " - " + maxPrice.ToString();
                if (productList[i].storesWithPricesAndLinks.Count == 1)
                    lblPrice.Text += " (" + productList[i].storesWithPricesAndLinks.Count + " store)";
                else
                    lblPrice.Text += " (" + productList[i].storesWithPricesAndLinks.Count + " stores)";
                lblPrice.Width = lblPrice.Text.Length * 11;
                newProductPanel.Controls.Add(lblPrice);

                Button btnDetails = new Button();
                btnDetails.Text = "Show details";
                btnDetails.Name = "btnDetails" + i.ToString();
                btnDetails.Width = btnDetails.Text.Length * 5 + 20;
                btnDetails.Click += BtnDetails_Click;
                newProductPanel.Controls.Add(btnDetails);
            }
        }
        private void ClearSearch()
        {
            this.flpProductView.Controls.Clear();
            this.flpStoresList.Controls.Clear();
        }
        private void BtnDetails_Click(object sender, EventArgs e)
        {
            flpProductDetails.Controls.Clear();
            flpStoresList.Controls.Clear();
            ShowDetails(this.productsToShow[int.Parse(((Button)sender).Name.TrimStart("btnDetails".ToCharArray()))]);
        }
        private void ShowDetails(ProductWithStoresAndPrices productWStoresAndPrices)
        {
            //this.productsForCart = new List<ProductWStorePriceAndQuantity>();
            Label lblChosenProductName = new Label();
            lblChosenProductName.Text = "Name: " + productWStoresAndPrices.product.name;
            flpProductDetails.Controls.Add(lblChosenProductName);

            float minPrice = 1000000, maxPrice = 0;
            foreach (StoreWithPriceAndLink store in productWStoresAndPrices.storesWithPricesAndLinks)
            {
                if (store.price < minPrice)
                    minPrice = store.price;
                if (store.price > maxPrice)
                    maxPrice = store.price;
            }

            Label lblPriceRange = new Label();
            if (minPrice == maxPrice)
                lblPriceRange.Text = "Price: " + minPrice.ToString();
            else
                lblPriceRange.Text = "Price: " + minPrice.ToString() + " - " + maxPrice.ToString();
            if (productWStoresAndPrices.storesWithPricesAndLinks.Count == 1)
                lblPriceRange.Text += " (" + productWStoresAndPrices.storesWithPricesAndLinks.Count + " store)";
            else
                lblPriceRange.Text += " (" + productWStoresAndPrices.storesWithPricesAndLinks.Count + " stores)";
            lblPriceRange.Width = lblPriceRange.Text.Length * 10;
            flpProductDetails.Controls.Add(lblPriceRange);
            flpStoresList.Controls.Clear();
            this.chosenProduct = new ProductWStorePriceAndQuantity()
            {
                product = productWStoresAndPrices.product,
                quantity = 1
            };
            productWStoresAndPrices.storesWithPricesAndLinks = productWStoresAndPrices.storesWithPricesAndLinks.OrderBy( x => x.price).ToList();
            for (int i = 0; i < productWStoresAndPrices.storesWithPricesAndLinks.Count; i++)
            {
                FlowLayoutPanel storeContainer = new FlowLayoutPanel();
                storeContainer.Width = flpStoresList.Width;
                storeContainer.Controls.Add(new Label() { Width = storeContainer.Width, Height = 1, Dock = DockStyle.Bottom, BackColor = Color.DimGray });
                //store details
                FlowLayoutPanel flpStoreDetails = new FlowLayoutPanel();
                flpStoreDetails.AutoScroll = true;
                flpStoreDetails.FlowDirection = FlowDirection.TopDown;
                flpStoreDetails.Width = storeContainer.Width;
                flpStoreDetails.Height = (int)(storeContainer.Height * 0.5);
                Label storeName = new Label();
                storeName.Text = "Store name: " + productWStoresAndPrices.storesWithPricesAndLinks[i].store.name;
                storeName.Width = storeName.Text.Length * 10;
                flpStoreDetails.Controls.Add(storeName);

                Label lblPrice = new Label();
                lblPrice.Text = "Price: " + productWStoresAndPrices.storesWithPricesAndLinks[i].price.ToString();
                flpStoreDetails.Controls.Add(lblPrice);

                //shopping list controls
                //FlowLayoutPanel flpShoppingControls = new FlowLayoutPanel();
                //flpShoppingControls.Anchor = AnchorStyles.Right;
                //flpShoppingControls.Width = storeContainer.Width - 30;
                //flpShoppingControls.Height = storeContainer.Height;
                //flpShoppingControls.FlowDirection = FlowDirection.LeftToRight;

                //NumericUpDown numQuantity = new NumericUpDown();
                //numQuantity.Name = "numQuantity" + i.ToString();
                //numQuantity.Minimum = 1;
                //numQuantity.Maximum = 10000;
                //numQuantity.Width = (int)(flpShoppingControls.Width * 0.4);
                //numQuantity.ValueChanged += NumQuantity_ValueChanged;
                //flpShoppingControls.Controls.Add(numQuantity);

                //Button btnAddToShoppingList = new Button();
                //btnAddToShoppingList.Name = "btnAddToCart" + i.ToString();
                //btnAddToShoppingList.Text = "Add to cart";
                //btnAddToShoppingList.Height = 25;
                //btnAddToShoppingList.Width = (int)(flpShoppingControls.Width * 0.4);
                //btnAddToShoppingList.Click += BtnAddToShoppingList_Click;
                //flpShoppingControls.Controls.Add(btnAddToShoppingList);


                storeContainer.Controls.Add(flpStoreDetails);
                //storeContainer.Controls.Add(flpShoppingControls);

                flpStoresList.Controls.Add(storeContainer);

                //productsForCart.Add(new ProductWStorePriceAndQuantity
                //    {
                //        product = productWStoresAndPrices.product,
                //        store = productWStoresAndPrices.storesWithPricesAndLinks[i].store,
                //        price = productWStoresAndPrices.storesWithPricesAndLinks[i].price,
                //        quantity = (int)numQuantity.Value
                //    }
                //);
            }
        }

        private void NumQuantity_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown num = (NumericUpDown)sender;
            int changedIndex = int.Parse(num.Name.TrimStart("numQuantity".ToCharArray()));
            productsForCart[changedIndex].quantity = (int)num.Value;
        }

        private void BtnAddToShoppingList_Click(object sender, EventArgs e)
        {
            int selIndex = int.Parse(((Button)sender).Name.TrimStart("btnAddToCart".ToCharArray()));
            this.client.Cypher.Match("(product:Product { name: '" + productsForCart[selIndex].product.name + "'})")
                              .Match("(user:User { email: '" + this.email + "'})")
                              .Merge("(user)-[si:IS_SHOPPING_ITEM ]->(product)")
                              .Set("si.storeName = '" + productsForCart[selIndex].store.name
                                    + "', si.quantity = " + productsForCart[selIndex].quantity.ToString()
                                    + ", si.price = " + productsForCart[selIndex].price.ToString())
                              .ExecuteWithoutResults();
            MessageBox.Show(productsForCart[selIndex].quantity.ToString() + "x" + productsForCart[selIndex].price.ToString());
        }

        private List<String> GetTypesForFiltering()
        {
            List<String> res = new List<String>();
            foreach (var item in flpTypes.Controls)
            {
                CheckBox cb = (CheckBox)item;
                if (cb.Checked)
                {
                    res.Add(cb.Text);
                }
            }
            if (res.Count > 0)
                return res;
            this.typeList.ForEach(x => res.Add(x.name));
            return res;
        }

        private void ClientForm_Load(object sender, EventArgs e)
        {
            InitializeTypeList();
        }

        private void tcClientForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (tcClientForm.SelectedIndex == 1)
            //{
            //    LoadShoppingCart();
            //}
        }
        //private void LoadShoppingCart()
        //{
        //    List<ProductWStorePriceAndQuantity> cartItems = this.client.Cypher.Match("(user:User { email: '" + this.email + "'})-[shoppingItem:IS_SHOPPING_ITEM]->(product:Product)<-[sells:SELLS]-(store:Store)")
        //                                                                      .With("{ product: product, store: store, price: shoppingItem.price, quantity: shoppingItem.quantity } as res")
        //                                                                      .Return<ProductWStorePriceAndQuantity>("res")
        //                                                                      .Results
        //                                                                      .ToList();

        //}
        private void numLower_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }
    }
}
