namespace PRNFE.MVC.Models.EnumClass
{
	public class EnumClass
	{
	}

	public enum VehicleTypes
	{
		Motorbike = 0,
		Bicycle = 1,
		Car = 2,
		SmallTruck = 3,
		ElectricBike = 4,
		Other = 5
	}
	public enum BillStatusDat
	{
		Created = 0,
		WaitPay = 1,
		Paid = 2,
		Overdue = 3,
		Cancelled = 4
	}

	public enum InvoiceStatus
	{
		Pending = 0,
		Paid = 1,
		Overdue = 2,
		Cancelled = 3
	}

	public enum PaymentMethod
	{
		Cash = 0,
		BankTransfer = 1,
		Momo = 2,
		ZaloPay = 3,
		CreditCard = 4
	}

}
