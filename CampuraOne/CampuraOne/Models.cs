using System;

namespace CampuraOne
{
    public class Merchant
    {
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        // 父商户ID，null 表示顶级商户
        public int? ParentMerchantID { get; set; }
    }
    public class Product
    {
        public int ProductID { get; set; }
        public int MerchantID { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
    }
    public class Promotion
    {
        public int PromotionID { get; set; }
        public int ProductID { get; set; }
        public string PromotionTitle { get; set; }
        public string PromotionContent { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
    public class Advertisement
    {
        public int AdvertisementID { get; set; }
        public int PromotionID { get; set; }
        public string AdTitle { get; set; }
        public string ImagePath { get; set; }
        public bool IsEnabled { get; set; }
    }
}