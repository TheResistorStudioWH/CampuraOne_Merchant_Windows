using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CampuraOne
{
    public partial class MainForm : Form
    {
        private List<Merchant> merchants = new List<Merchant>();
        private List<Product> products = new List<Product>();
        private List<Promotion> promotions = new List<Promotion>();
        private List<Advertisement> advertisements = new List<Advertisement>();

        private int merchantId = 1;
        private int productId = 1;
        private int promotionId = 1;
        private int advertisementId = 1;

        private TabControl tabControl;

        // 仪表盘 / 商户管理控件
        private ComboBox cmbCurrentMerchant;
        private Label lblCurrentMerchantName;
        private Label lblCurrentMerchantPhone;
        private Label lblCurrentMerchantAddress;

        private TextBox txtMerchantName;
        private TextBox txtMerchantPhone;
        private TextBox txtMerchantAddress;
        private DataGridView dgvMerchants;
        private DataGridView dgvAffiliates;
        private DataGridView dgvUpcomingPromotions;
        private DataGridView dgvUpcomingAdvertisements;

        private ComboBox cmbProductMerchant;
        private TextBox txtProductName;
        private TextBox txtProductPrice;
        private DataGridView dgvProducts;

        private ComboBox cmbPromotionMerchant;
        private ComboBox cmbPromotionProduct;
        private TextBox txtPromotionTitle;
        private TextBox txtPromotionContent;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private DataGridView dgvPromotions;

        private ComboBox cmbAdMerchant;
        private ComboBox cmbAdProduct;
        private ComboBox cmbAdPromotion;
        private TextBox txtAdTitle;
        private TextBox txtAdImagePath;
        private CheckBox chkAdEnabled;
        private DataGridView dgvAdvertisements;

        public MainForm()
        {
            InitializeComponent();

            BuildMainPage();

            this.FormClosed += MainForm_FormClosed;
        }

        private void RefreshMerchantDashboard()
        {
            if (cmbCurrentMerchant == null)
                return;

            Merchant current = cmbCurrentMerchant.SelectedItem as Merchant;

            if (current == null)
            {
                lblCurrentMerchantName.Text = "商户名称: -";
                lblCurrentMerchantPhone.Text = "电话: -";
                lblCurrentMerchantAddress.Text = "地址: -";

                dgvUpcomingPromotions.DataSource = null;
                dgvUpcomingAdvertisements.DataSource = null;
                dgvAffiliates.DataSource = null;
                return;
            }

            lblCurrentMerchantName.Text = "商户名称: " + current.MerchantName;
            lblCurrentMerchantPhone.Text = "电话: " + current.Phone;
            lblCurrentMerchantAddress.Text = "地址: " + current.Address;

            // 临近结束促销：属于当前商户的商品的促销，且结束时间在未来7天内
            DateTime now = DateTime.Now.Date;
            DateTime limit = now.AddDays(7);

            var productIds = products.Where(p => p.MerchantID == current.MerchantID).Select(p => p.ProductID).ToList();

            var upcomingPromos = promotions.Where(p => productIds.Contains(p.ProductID) && p.EndTime >= now && p.EndTime <= limit)
                .Select(p => new
                {
                    编号 = p.PromotionID,
                    所属商品 = products.FirstOrDefault(x => x.ProductID == p.ProductID)?.ProductName ?? "",
                    促销标题 = p.PromotionTitle,
                    结束时间 = p.EndTime.ToShortDateString()
                }).ToList();

            dgvUpcomingPromotions.DataSource = upcomingPromos;

            // 临近结束广告：广告关联的促销其结束时间在未来7天内
            var upcomingAds = advertisements.Where(a =>
            {
                var promo = promotions.FirstOrDefault(p => p.PromotionID == a.PromotionID);
                if (promo == null) return false;
                var prod = products.FirstOrDefault(pr => pr.ProductID == promo.ProductID);
                if (prod == null) return false;
                return prod.MerchantID == current.MerchantID && promo.EndTime >= now && promo.EndTime <= limit;
            }).Select(a =>
            {
                var promo = promotions.FirstOrDefault(p => p.PromotionID == a.PromotionID);
                var prod = products.FirstOrDefault(pr => pr.ProductID == promo.ProductID);
                return new
                {
                    编号 = a.AdvertisementID,
                    所属商品 = prod?.ProductName ?? "",
                    所属促销 = promo?.PromotionTitle ?? "",
                    广告标题 = a.AdTitle,
                    结束时间 = promo?.EndTime.ToShortDateString() ?? ""
                };
            }).ToList();

            dgvUpcomingAdvertisements.DataSource = upcomingAds;

            // 附属商户
            var affiliates = merchants.Where(m => m.ParentMerchantID == current.MerchantID)
                .Select(m => new
                {
                    编号 = m.MerchantID,
                    商户名称 = m.MerchantName,
                    电话 = m.Phone,
                    地址 = m.Address
                }).ToList();

            dgvAffiliates.DataSource = affiliates;
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void BuildMainPage()
        {
            this.Text = "CampuraOne 管理页面";
            this.Width = 1100;
            this.Height = 700;

            this.Controls.Clear();

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            this.Controls.Add(tabControl);

            BuildMerchantTab();
            BuildProductTab();
            BuildPromotionTab();
            BuildAdvertisementTab();

            RefreshAll();
        }

        private void BuildMerchantTab()
        {
            TabPage tabPage = new TabPage("商户仪表盘");
            tabControl.TabPages.Add(tabPage);

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 3;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            // 顶部：选择当前商户与详情
            FlowLayoutPanel topPanel = CreateTopPanel();

            cmbCurrentMerchant = CreateComboBox();
            cmbCurrentMerchant.Width = 200;
            cmbCurrentMerchant.SelectedIndexChanged += (s, e) => RefreshMerchantDashboard();

            lblCurrentMerchantName = CreateLabel("商户名称: -");
            lblCurrentMerchantPhone = CreateLabel("电话: -");
            lblCurrentMerchantAddress = CreateLabel("地址: -");

            topPanel.Controls.Add(CreateLabel("当前商户"));
            topPanel.Controls.Add(cmbCurrentMerchant);
            topPanel.Controls.Add(lblCurrentMerchantName);
            topPanel.Controls.Add(lblCurrentMerchantPhone);
            topPanel.Controls.Add(lblCurrentMerchantAddress);

            // 中部：临近结束的促销和广告
            FlowLayoutPanel midPanel = new FlowLayoutPanel();
            midPanel.Dock = DockStyle.Fill;
            midPanel.AutoSize = false;
            midPanel.WrapContents = false;
            midPanel.Padding = new Padding(10);
            midPanel.FlowDirection = FlowDirection.LeftToRight;

            GroupBox gbPromotions = new GroupBox();
            gbPromotions.Text = "临近结束的促销 (7天内)";
            gbPromotions.Width = 520;
            gbPromotions.Height = 220;
            dgvUpcomingPromotions = CreateGrid();
            dgvUpcomingPromotions.Dock = DockStyle.Fill;
            gbPromotions.Controls.Add(dgvUpcomingPromotions);

            GroupBox gbAds = new GroupBox();
            gbAds.Text = "临近结束的广告位 (关联促销结束)";
            gbAds.Width = 520;
            gbAds.Height = 220;
            dgvUpcomingAdvertisements = CreateGrid();
            dgvUpcomingAdvertisements.Dock = DockStyle.Fill;
            gbAds.Controls.Add(dgvUpcomingAdvertisements);

            midPanel.Controls.Add(gbPromotions);
            midPanel.Controls.Add(gbAds);

            // 底部：附属商户管理
            FlowLayoutPanel bottomPanel = CreateTopPanel();

            txtMerchantName = CreateTextBox(130);
            txtMerchantPhone = CreateTextBox(130);
            txtMerchantAddress = CreateTextBox(200);

            Button btnAddMerchant = new Button();
            btnAddMerchant.Text = "添加附属商户";
            btnAddMerchant.Width = 120;
            btnAddMerchant.Click += BtnAddMerchant_Click;

            bottomPanel.Controls.Add(CreateLabel("附属商户名称"));
            bottomPanel.Controls.Add(txtMerchantName);
            bottomPanel.Controls.Add(CreateLabel("电话"));
            bottomPanel.Controls.Add(txtMerchantPhone);
            bottomPanel.Controls.Add(CreateLabel("地址"));
            bottomPanel.Controls.Add(txtMerchantAddress);
            bottomPanel.Controls.Add(btnAddMerchant);

            dgvAffiliates = CreateGrid();
            dgvAffiliates.Height = 180;

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(midPanel, 0, 1);
            layout.Controls.Add(bottomPanel, 0, 2);

            // 将下方的附属表格放到 tabPage 的底部
            Panel bottomWrapper = new Panel();
            bottomWrapper.Dock = DockStyle.Bottom;
            bottomWrapper.Height = 200;
            bottomWrapper.Padding = new Padding(10);
            bottomWrapper.Controls.Add(dgvAffiliates);

            tabPage.Controls.Add(layout);
            tabPage.Controls.Add(bottomWrapper);
        }

        private void BuildProductTab()
        {
            TabPage tabPage = new TabPage("商品管理");
            tabControl.TabPages.Add(tabPage);

            TableLayoutPanel layout = CreateBaseLayout();
            FlowLayoutPanel topPanel = CreateTopPanel();

            cmbProductMerchant = CreateComboBox();
            txtProductName = CreateTextBox(130);
            txtProductPrice = CreateTextBox(80);

            Button btnAddProduct = new Button();
            btnAddProduct.Text = "添加商品";
            btnAddProduct.Width = 100;
            btnAddProduct.Click += BtnAddProduct_Click;

            topPanel.Controls.Add(CreateLabel("选择商户"));
            topPanel.Controls.Add(cmbProductMerchant);
            topPanel.Controls.Add(CreateLabel("商品名称"));
            topPanel.Controls.Add(txtProductName);
            topPanel.Controls.Add(CreateLabel("价格"));
            topPanel.Controls.Add(txtProductPrice);
            topPanel.Controls.Add(btnAddProduct);

            dgvProducts = CreateGrid();

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(dgvProducts, 0, 1);

            tabPage.Controls.Add(layout);
        }

        private void BuildPromotionTab()
        {
            TabPage tabPage = new TabPage("促销管理");
            tabControl.TabPages.Add(tabPage);

            TableLayoutPanel layout = CreateBaseLayout();
            FlowLayoutPanel topPanel = CreateTopPanel();

            cmbPromotionMerchant = CreateComboBox();
            cmbPromotionProduct = CreateComboBox();

            cmbPromotionMerchant.SelectedIndexChanged += CmbPromotionMerchant_SelectedIndexChanged;

            txtPromotionTitle = CreateTextBox(130);
            txtPromotionContent = CreateTextBox(200);

            dtpStart = new DateTimePicker();
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.Width = 110;

            dtpEnd = new DateTimePicker();
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Width = 110;

            Button btnAddPromotion = new Button();
            btnAddPromotion.Text = "添加促销";
            btnAddPromotion.Width = 100;
            btnAddPromotion.Click += BtnAddPromotion_Click;

            topPanel.Controls.Add(CreateLabel("选择商户"));
            topPanel.Controls.Add(cmbPromotionMerchant);
            topPanel.Controls.Add(CreateLabel("选择商品"));
            topPanel.Controls.Add(cmbPromotionProduct);
            topPanel.Controls.Add(CreateLabel("促销标题"));
            topPanel.Controls.Add(txtPromotionTitle);
            topPanel.Controls.Add(CreateLabel("促销内容"));
            topPanel.Controls.Add(txtPromotionContent);
            topPanel.Controls.Add(CreateLabel("开始"));
            topPanel.Controls.Add(dtpStart);
            topPanel.Controls.Add(CreateLabel("结束"));
            topPanel.Controls.Add(dtpEnd);
            topPanel.Controls.Add(btnAddPromotion);

            dgvPromotions = CreateGrid();

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(dgvPromotions, 0, 1);

            tabPage.Controls.Add(layout);
        }

        private void BuildAdvertisementTab()
        {
            TabPage tabPage = new TabPage("广告位管理");
            tabControl.TabPages.Add(tabPage);

            TableLayoutPanel layout = CreateBaseLayout();
            FlowLayoutPanel topPanel = CreateTopPanel();

            cmbAdMerchant = CreateComboBox();
            cmbAdProduct = CreateComboBox();
            cmbAdPromotion = CreateComboBox();

            cmbAdMerchant.SelectedIndexChanged += CmbAdMerchant_SelectedIndexChanged;
            cmbAdProduct.SelectedIndexChanged += CmbAdProduct_SelectedIndexChanged;

            txtAdTitle = CreateTextBox(130);
            txtAdImagePath = CreateTextBox(200);

            chkAdEnabled = new CheckBox();
            chkAdEnabled.Text = "启用";
            chkAdEnabled.Checked = true;
            chkAdEnabled.AutoSize = true;

            Button btnAddAd = new Button();
            btnAddAd.Text = "添加广告位";
            btnAddAd.Width = 100;
            btnAddAd.Click += BtnAddAd_Click;

            topPanel.Controls.Add(CreateLabel("选择商户"));
            topPanel.Controls.Add(cmbAdMerchant);
            topPanel.Controls.Add(CreateLabel("选择商品"));
            topPanel.Controls.Add(cmbAdProduct);
            topPanel.Controls.Add(CreateLabel("选择促销"));
            topPanel.Controls.Add(cmbAdPromotion);
            topPanel.Controls.Add(CreateLabel("广告标题"));
            topPanel.Controls.Add(txtAdTitle);
            topPanel.Controls.Add(CreateLabel("图片路径"));
            topPanel.Controls.Add(txtAdImagePath);
            topPanel.Controls.Add(chkAdEnabled);
            topPanel.Controls.Add(btnAddAd);

            dgvAdvertisements = CreateGrid();

            layout.Controls.Add(topPanel, 0, 0);
            layout.Controls.Add(dgvAdvertisements, 0, 1);

            tabPage.Controls.Add(layout);
        }

        private TableLayoutPanel CreateBaseLayout()
        {
            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 2;
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            return layout;
        }

        private FlowLayoutPanel CreateTopPanel()
        {
            FlowLayoutPanel panel = new FlowLayoutPanel();
            panel.Dock = DockStyle.Fill;
            panel.AutoSize = true;
            panel.WrapContents = true;
            panel.Padding = new Padding(10);
            return panel;
        }

        private Label CreateLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.AutoSize = true;
            label.Margin = new Padding(10, 8, 3, 3);
            return label;
        }

        private TextBox CreateTextBox(int width)
        {
            TextBox textBox = new TextBox();
            textBox.Width = width;
            return textBox;
        }

        private ComboBox CreateComboBox()
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Width = 150;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            return comboBox;
        }

        private DataGridView CreateGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            return grid;
        }

        private void BtnAddMerchant_Click(object sender, EventArgs e)
        {
            // 添加的是附属商户，ParentMerchant 为当前选中的商户
            string name = txtMerchantName.Text.Trim();

            if (cmbCurrentMerchant.SelectedItem == null)
            {
                MessageBox.Show("请先选择当前商户（父商户）");
                return;
            }

            if (name == "")
            {
                MessageBox.Show("请输入附属商户名称");
                return;
            }

            Merchant parent = cmbCurrentMerchant.SelectedItem as Merchant;

            Merchant merchant = new Merchant();
            merchant.MerchantID = merchantId++;
            merchant.MerchantName = name;
            merchant.Phone = txtMerchantPhone.Text.Trim();
            merchant.Address = txtMerchantAddress.Text.Trim();
            merchant.ParentMerchantID = parent?.MerchantID;

            merchants.Add(merchant);

            txtMerchantName.Clear();
            txtMerchantPhone.Clear();
            txtMerchantAddress.Clear();

            RefreshAll();

            MessageBox.Show("附属商户添加成功");
        }

        private void BtnAddProduct_Click(object sender, EventArgs e)
        {
            Merchant merchant = cmbProductMerchant.SelectedItem as Merchant;

            if (merchant == null)
            {
                MessageBox.Show("请先添加并选择商户");
                return;
            }

            string name = txtProductName.Text.Trim();

            if (name == "")
            {
                MessageBox.Show("请输入商品名称");
                return;
            }

            decimal price;

            if (!decimal.TryParse(txtProductPrice.Text.Trim(), out price))
            {
                MessageBox.Show("请输入正确的价格");
                return;
            }

            Product product = new Product();
            product.ProductID = productId++;
            product.MerchantID = merchant.MerchantID;
            product.ProductName = name;
            product.Price = price;

            products.Add(product);

            txtProductName.Clear();
            txtProductPrice.Clear();

            RefreshAll();

            MessageBox.Show("商品添加成功");
        }

        private void BtnAddPromotion_Click(object sender, EventArgs e)
        {
            Product product = cmbPromotionProduct.SelectedItem as Product;

            if (product == null)
            {
                MessageBox.Show("请先选择商品");
                return;
            }

            string title = txtPromotionTitle.Text.Trim();

            if (title == "")
            {
                MessageBox.Show("请输入促销标题");
                return;
            }

            if (dtpEnd.Value.Date < dtpStart.Value.Date)
            {
                MessageBox.Show("结束时间不能早于开始时间");
                return;
            }

            Promotion promotion = new Promotion();
            promotion.PromotionID = promotionId++;
            promotion.ProductID = product.ProductID;
            promotion.PromotionTitle = title;
            promotion.PromotionContent = txtPromotionContent.Text.Trim();
            promotion.StartTime = dtpStart.Value.Date;
            promotion.EndTime = dtpEnd.Value.Date;

            promotions.Add(promotion);

            txtPromotionTitle.Clear();
            txtPromotionContent.Clear();

            RefreshAll();

            MessageBox.Show("促销添加成功");
        }

        private void BtnAddAd_Click(object sender, EventArgs e)
        {
            Promotion promotion = cmbAdPromotion.SelectedItem as Promotion;

            if (promotion == null)
            {
                MessageBox.Show("请先选择促销");
                return;
            }

            string title = txtAdTitle.Text.Trim();

            if (title == "")
            {
                MessageBox.Show("请输入广告标题");
                return;
            }

            Advertisement ad = new Advertisement();
            ad.AdvertisementID = advertisementId++;
            ad.PromotionID = promotion.PromotionID;
            ad.AdTitle = title;
            ad.ImagePath = txtAdImagePath.Text.Trim();
            ad.IsEnabled = chkAdEnabled.Checked;

            advertisements.Add(ad);

            txtAdTitle.Clear();
            txtAdImagePath.Clear();
            chkAdEnabled.Checked = true;

            RefreshAll();

            MessageBox.Show("广告位添加成功");
        }

        private void CmbPromotionMerchant_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPromotionProductComboBox();
        }

        private void CmbAdMerchant_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshAdProductComboBox();
        }

        private void CmbAdProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshAdPromotionComboBox();
        }

        private void RefreshAll()
        {
            RefreshMerchantComboBoxes();
            RefreshDashboardCombobox();
            RefreshPromotionProductComboBox();
            RefreshAdProductComboBox();
            RefreshAdPromotionComboBox();

            RefreshMerchantGrid();
            RefreshProductGrid();
            RefreshPromotionGrid();
            RefreshAdvertisementGrid();

            RefreshMerchantDashboard();
        }

        private void RefreshMerchantComboBoxes()
        {
            BindMerchantComboBox(cmbProductMerchant);
            BindMerchantComboBox(cmbPromotionMerchant);
            BindMerchantComboBox(cmbAdMerchant);
            BindMerchantComboBox(cmbCurrentMerchant);
        }

        private void RefreshDashboardCombobox()
        {
            if (cmbCurrentMerchant == null)
                return;

            cmbCurrentMerchant.DataSource = null;
            cmbCurrentMerchant.DisplayMember = "MerchantName";
            cmbCurrentMerchant.ValueMember = "MerchantID";
            // 仅显示顶级或所有商户作为可登录的当前商户，取 merchants 的浅拷贝
            cmbCurrentMerchant.DataSource = merchants.ToList();
        }

        private void BindMerchantComboBox(ComboBox comboBox)
        {
            if (comboBox == null)
                return;

            comboBox.DataSource = null;
            comboBox.DisplayMember = "MerchantName";
            comboBox.ValueMember = "MerchantID";
            comboBox.DataSource = merchants.ToList();
        }

        private void RefreshPromotionProductComboBox()
        {
            if (cmbPromotionMerchant == null || cmbPromotionProduct == null)
                return;

            Merchant merchant = cmbPromotionMerchant.SelectedItem as Merchant;

            List<Product> list = new List<Product>();

            if (merchant != null)
            {
                list = products.Where(p => p.MerchantID == merchant.MerchantID).ToList();
            }

            cmbPromotionProduct.DataSource = null;
            cmbPromotionProduct.DisplayMember = "ProductName";
            cmbPromotionProduct.ValueMember = "ProductID";
            cmbPromotionProduct.DataSource = list;
        }

        private void RefreshAdProductComboBox()
        {
            if (cmbAdMerchant == null || cmbAdProduct == null)
                return;

            Merchant merchant = cmbAdMerchant.SelectedItem as Merchant;

            List<Product> list = new List<Product>();

            if (merchant != null)
            {
                list = products.Where(p => p.MerchantID == merchant.MerchantID).ToList();
            }

            cmbAdProduct.DataSource = null;
            cmbAdProduct.DisplayMember = "ProductName";
            cmbAdProduct.ValueMember = "ProductID";
            cmbAdProduct.DataSource = list;
        }

        private void RefreshAdPromotionComboBox()
        {
            if (cmbAdProduct == null || cmbAdPromotion == null)
                return;

            Product product = cmbAdProduct.SelectedItem as Product;

            List<Promotion> list = new List<Promotion>();

            if (product != null)
            {
                list = promotions.Where(p => p.ProductID == product.ProductID).ToList();
            }

            cmbAdPromotion.DataSource = null;
            cmbAdPromotion.DisplayMember = "PromotionTitle";
            cmbAdPromotion.ValueMember = "PromotionID";
            cmbAdPromotion.DataSource = list;
        }

        private void RefreshMerchantGrid()
        {
            if (dgvMerchants == null)
                return;

            dgvMerchants.DataSource = null;

            dgvMerchants.DataSource = merchants.Select(m => new
            {
                编号 = m.MerchantID,
                商户名称 = m.MerchantName,
                电话 = m.Phone,
                地址 = m.Address,
                父商户 = m.ParentMerchantID == null ? "-" : (merchants.FirstOrDefault(x => x.MerchantID == m.ParentMerchantID)?.MerchantName ?? "-")
            }).ToList();
        }

        private void RefreshProductGrid()
        {
            dgvProducts.DataSource = null;

            dgvProducts.DataSource = products.Select(p =>
            {
                Merchant merchant = merchants.FirstOrDefault(m => m.MerchantID == p.MerchantID);

                return new
                {
                    编号 = p.ProductID,
                    所属商户 = merchant == null ? "" : merchant.MerchantName,
                    商品名称 = p.ProductName,
                    价格 = p.Price
                };
            }).ToList();
        }

        private void RefreshPromotionGrid()
        {
            dgvPromotions.DataSource = null;

            dgvPromotions.DataSource = promotions.Select(p =>
            {
                Product product = products.FirstOrDefault(x => x.ProductID == p.ProductID);

                Merchant merchant = null;

                if (product != null)
                {
                    merchant = merchants.FirstOrDefault(m => m.MerchantID == product.MerchantID);
                }

                return new
                {
                    编号 = p.PromotionID,
                    所属商户 = merchant == null ? "" : merchant.MerchantName,
                    所属商品 = product == null ? "" : product.ProductName,
                    促销标题 = p.PromotionTitle,
                    促销内容 = p.PromotionContent,
                    开始时间 = p.StartTime.ToShortDateString(),
                    结束时间 = p.EndTime.ToShortDateString()
                };
            }).ToList();
        }

        private void RefreshAdvertisementGrid()
        {
            dgvAdvertisements.DataSource = null;

            dgvAdvertisements.DataSource = advertisements.Select(a =>
            {
                Promotion promotion = promotions.FirstOrDefault(p => p.PromotionID == a.PromotionID);

                Product product = null;
                Merchant merchant = null;

                if (promotion != null)
                {
                    product = products.FirstOrDefault(p => p.ProductID == promotion.ProductID);
                }

                if (product != null)
                {
                    merchant = merchants.FirstOrDefault(m => m.MerchantID == product.MerchantID);
                }

                return new
                {
                    编号 = a.AdvertisementID,
                    所属商户 = merchant == null ? "" : merchant.MerchantName,
                    所属商品 = product == null ? "" : product.ProductName,
                    所属促销 = promotion == null ? "" : promotion.PromotionTitle,
                    广告标题 = a.AdTitle,
                    图片路径 = a.ImagePath,
                    是否启用 = a.IsEnabled
                };
            }).ToList();
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }
    }

    

    
}