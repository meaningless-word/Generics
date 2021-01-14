using System;

namespace Generics
{
    enum OrderStatus
    {
        confirmation = 0,
        preparation,
        packaging,
        shipment,
        complete
    }
    enum DeliveryStatus 
    {
        just_transferred = 0,
        delivering,
        waiting,
        received
    }

    //заказ
    class Order<TDelivery, TStruct> where TDelivery : Delivery
    {
        private string number; //номер заказа
 
        public string Number { get { return number; } } 
        public Order(string number)
        {
            this.number = number;
        }
        public Order(int number)
        {
            this.number = number.ToString();
        }
        public Order(DateTime number)
        {
            this.number = number.ToString("dd.MM.yyyy HH:mm:ss");
        }

        public OrderStatus status; //статус заказа
        public void Confirm() //подтверждение заказа означает начало подготовки заказа
        {
            if (status == OrderStatus.confirmation)
            {
                if (items != null)
                {
                    Console.WriteLine("Ваш заказ содержит:");
                    ProductList();
                    status++;
                    Console.WriteLine("подтверждение получено\n");
                }
                else
                {
                    Console.WriteLine("заказ пустой, добавьте товар, хотя бы один...");
                    Console.WriteLine("подтверждение отклонено\n");
                }
            }
        }
        public void GetStatus() //интерес к статусу заказа
        {
            if (delivery.GetType() == typeof(ShopDelivery))
            {
                status = OrderStatus.shipment; //выдача из магазина пусть начинается сразу с выдачи безо всяких доставок - упакковок
            }

            ShowStatus();

            //для простоты повесим на запрос о статусе заказа переход к следующей стадии
            if ((status > OrderStatus.confirmation && status < OrderStatus.shipment) || 
                (status == OrderStatus.shipment && delivery.status == DeliveryStatus.received))
            {
                status++;
            }
        }
        private void ShowStatus()
        {
            Console.Write("заказ №{0} ", Number);
            switch (status)
            {
                case OrderStatus.confirmation:
                    Console.WriteLine("ожидает подтверждения");
                    break;
                case OrderStatus.preparation:
                    Console.WriteLine("на стадии сборки");
                    break;
                case OrderStatus.packaging:
                    Console.WriteLine("бережно упаковывается");
                    break;
                case OrderStatus.shipment:
                    delivery.GetStatus(); 
                    break;
                case OrderStatus.complete:
                    delivery.GetStatus();
                    break;
            }
        }

        private TDelivery delivery; //доставка
        public void ToDelivery (TDelivery delivery)
        {
            this.delivery = delivery;
        }

        private TStruct[] items;
        public void AddProduct (TStruct p)
        {
            if (items == null)
            {
                items = new TStruct[1];
                items[0] = p;
            } 
            else
            {
                Array.Resize(ref items, items.Length + 1);
                items[items.Length - 1] = p;
            }
        }
        protected void ProductList()
        {
            if (items == null)
            {
                Console.WriteLine("выводить-то нечего...");
            }
            else
            {
                Console.WriteLine("{0,-30}:{1,-50}", "-----продукт------------", "-----характеристики--------------------");
                foreach (TStruct p in items)
                {
                    Console.WriteLine(p.ToString());
                }
            }
        }
    }

    //коллекция заказов
    class OrderCollection
    {
        private Order<Delivery, Product>[] collection;
        public OrderCollection(Order<Delivery, Product>[] collection)
        {
            this.collection = collection;
        }

        // Индексатор по массиву
        public Order<Delivery, Product> this[int index]
        {
            get
            {
                // Проверяем, чтобы индекс был в диапазоне для массива
                if (index >= 0 && index < collection.Length)
                {
                    return collection[index];
                }
                else
                {
                    return null;
                }
            }

            private set
            {
                // Проверяем, чтобы индекс был в диапазоне для массива
                if (index >= 0 && index < collection.Length)
                {
                    collection[index] = value;
                }
            }
        }

        public Order<Delivery, Product> this[string number]
        {
            get
            {
                for (int i = 0; i < collection.Length; i++)
                {
                    if (collection[i].Number == number)
                    {
                        return collection[i];
                    }
                }

                return null;
            }
        }

        public int Length { get { return collection.Length; } }
    }

    //доставка
    abstract class Delivery
    {
        public string Address;
        public DeliveryStatus status;

        public Delivery(string address)
        {
            Address = address;
            status = DeliveryStatus.just_transferred;
        }
        public virtual void GetStatus() //интерес к статусу доставки
        {
            ShowStatus();
            //для простоты повесим на запрос о статусе доставки переход к следующей стадии
            if (status < DeliveryStatus.received)
            {
                status++;
            }
        }
        protected virtual void ShowStatus()
        {
            if (status == DeliveryStatus.just_transferred)
            {
                Console.WriteLine("только что передан службе доставки");
            }
        }


        public virtual void ShowAddress()
        {
            Console.WriteLine(Address);
        }
    }

    class HomeDelivery : Delivery
    {
        public HomeDelivery(string address) : base(address) { }

        public override void GetStatus()
        {
            base.GetStatus();
            if (status == DeliveryStatus.waiting)
            {
                status++; //потому что при доставке на дом нет режима ожидания
            }
        }

        protected override void ShowStatus()
        {
            base.ShowStatus();
            switch (status)
            {
                case DeliveryStatus.delivering:
                    Console.Write("доставляется по адресу: ");
                    ShowAddress();
                    break;
                case DeliveryStatus.received:
                    Console.WriteLine("торжественно вручён");
                    break;
            }
        }
    }

    class PickPointDelivery : Delivery
    {
        public PickPointDelivery(string address) : base(address) { }

        protected override void ShowStatus()
        {
            base.ShowStatus();
            switch (status)
            {
                case DeliveryStatus.delivering:
                    Console.WriteLine("отправлен в пункт выдачи");
                    break;
                case DeliveryStatus.waiting:
                    Console.Write("ожидает в пункте выдачи по адресу: ");
                    ShowAddress();
                    break;
                case DeliveryStatus.received:
                    Console.WriteLine("таинственно изъят");
                    break;
            }
        }
    }

    class ShopDelivery : Delivery
    {
        public ShopDelivery(string address) : base(address) 
        {
            status = DeliveryStatus.waiting; //пусть доставка в магазин сразу начинается с ожидания в магазине без лишней возни
        }

        public override void GetStatus()
        {
            base.GetStatus();
            status += status == DeliveryStatus.delivering ? 1 : 0; //пропустим доставку, потому что заказ уже ожидает в магазине
        }

        protected override void ShowStatus()
        {
            base.ShowStatus();
            switch (status)
            {
                case DeliveryStatus.waiting:
                    Console.Write("ждём не дождёмся в магазине по адресу: ");
                    ShowAddress();
                    break;
                case DeliveryStatus.received:
                    Console.WriteLine("весьма буднично принят");
                    break;
            }
        }
    }

    //товар
    struct Product
    {
        public string name { get; set; }
        public string features { get; set; }
        public override string ToString()
        {
            return String.Format("{0,-30}:{1,-50}", name, features);
        }
    }


    class Program
    {

        static void Main(string[] args)
        {
            //наполняется коллекция заказов
            var array = new Order<Delivery, Product>[] {
                new Order<Delivery, Product>(1),
                new Order<Delivery, Product>(DateTime.Now),
                new Order<Delivery, Product>("XC-12/01/2021")
            };
            OrderCollection Orders = new OrderCollection(array);

            //демонстрация заказа со способом достаки "на дом"
            Orders["1"].ToDelivery(new HomeDelivery("ул. Любая, 8"));
            Orders["1"].GetStatus(); //запрос статуса сразу после создания заказа с выбором способа доставки
            Orders["1"].GetStatus(); //с места не сдвинется, пока не подтвердится
            Orders["1"].Confirm(); //подтверждение заказа (но оно будет отклонено, поскольку заказ не содержит товаров)
            //поднакинем товаров
            Orders[0].AddProduct(new Product { name = "телевизор", features = "очень хороший" });
            Orders[0].AddProduct(new Product { name = "обогреватель", features = "ваще огонь" });
            Orders[0].AddProduct(new Product { name = "гарантийное обслуживание", features = "ну, такое себе..." });
            //снова попытаемся подтвердить заказ
            Orders["1"].Confirm();
            //следуя задумке о том, что каждый запрос статуса будет "толкать" заказ дальше по цепочке стадий обработки
            //запрашиваем в цикле статус заказа до тех пор, пока он не "выполнится"
            while (Orders["1"].status < OrderStatus.complete)
            {
                Orders["1"].GetStatus();
            }
            Orders["1"].GetStatus(); //контрольный запрос статуса, чтобы отобразилось "выполнен"
            Orders["1"].Confirm(); //демонстрация того, что подтверждение не "собьёт" статус на стадию подготовки
            Orders["1"].GetStatus(); //подтверждение, что заказ таки выполнен


            Console.ReadKey();
            Console.WriteLine();

            //демонстрация заказа со способом достаки "в пункт выдачи"
            Orders[1].ToDelivery(new PickPointDelivery("пер. Коробейный, д. 13, стр. 7"));
            Orders[1].GetStatus(); //запрос статуса сразу после создания заказа с выбором способа доставки
            Orders[1].AddProduct(new Product { name = "матрешка", features = "рекурсивная" });
            Orders[1].Confirm(); //подтверждение заказа
            //аналогично запрашиваем в цикле статус заказа до тех пор, пока он не "выполнится"
            while (Orders[1].status < OrderStatus.complete)
            {
                Orders[1].GetStatus();
            }
            Orders[1].GetStatus(); //контрольный запрос статуса, чтобы отобразилось "выполнен"

            Console.ReadKey();
            Console.WriteLine();

            //демонстрация заказа со способом достаки "в магазин"
            Orders[2].ToDelivery(new ShopDelivery("пр-т. Торговый, ТЦ \"Продажный\""));
            Orders[2].GetStatus(); //запрос статуса сразу после создания заказа с выбором способа доставки
            Orders[2].Confirm(); //подтверждение заказа
            //аналогично запрашиваем в цикле статус заказа до тех пор, пока он не "выполнится"
            while (Orders[2].status < OrderStatus.complete)
            {
                Orders[2].GetStatus();
            }
            Orders[2].GetStatus(); //контрольный запрос статуса, чтобы отобразилось "выполнен"

            Console.ReadKey();

        }
    }
}
