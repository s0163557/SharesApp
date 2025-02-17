using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stock_Analysis_Web_App.Models
{
    [Table("security_infos")]
    public class SecurityInfo
    {
        //Все имена полей представлены в том же виде, в каком они представлены в БД, только записаны в CamelCase, а не в snake_case
        [Key, Column("security_info_id")]
        public int SecurityInfoId { get; set; }
        //Уникальный строковый идентефикатор акции
        [Required, Column("security_id")]
        public string SecurityId { get; set; }
        //Полное название акции
        [Column("name")]
        public string? Name { get; set; }

        //12-символьный код ISIN
        [Column("isin")]
        public string? Isin { get; set; }
        //Количество выпущенных акций
        [Column("issue_size")]
        public long IssueSize { get; set; }
        //Дата выпуска акций
        [Column("issue_date")]
        public DateOnly IssueDate { get; set; }
        //Уровень листинга
        [Column("list_level")]
        public int ListLevel { get; set; }

        //Все связанные записи о торгах этой бумаги
        public List<SecurityTradeRecord> TradeRecords { get; set; } = new();

    }
}
