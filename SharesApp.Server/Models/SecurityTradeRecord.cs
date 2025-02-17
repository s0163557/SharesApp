using Stock_Analysis_Web_App.Classes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Stock_Analysis_Web_App.Models
{
    [Table("security_trade_records")]
    public class SecurityTradeRecord
    {
        [Key, Column("security_trade_record_id")]
        public int SecurityTradeRecordId { get; set; }
        //Ссылка на оригинальную акцию, записью о торгах которой является этот объект

        [Column("security_info_id"), Required]
        public SecurityInfo? SecurityInfo { get; set; } //Внешний ключ

        //Дата проведенных торгов
        [Column("date_of_trade")]
        public DateOnly DateOfTrade { get; set; }
        //Количество совершенных транзакций
        [Column("number_of_trades")]
        public int NumberOfTrades { get; set; }
        //
        [Column("value")]
        public double Value { get; set; }
        //Открывающая цена
        [Column("open")]
        public double Open { get; set; }
        //Нижняя предлагаемая цена
        [Column("low")]
        public double Low { get; set; }
        //Высшая предлагаемая цена
        [Column("high")]
        public double High { get; set; }
        //Закрывающая цена
        [Column("close")]
        public double Close { get; set; }
    }
}
