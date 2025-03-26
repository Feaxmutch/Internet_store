namespace InternetStore
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Good iPhone12 = new Good("IPhone 12");
            Good iPhone11 = new Good("IPhone 11");

            Warehouse warehouse = new Warehouse();

            Shop shop = new Shop(warehouse);

            warehouse.Delive(iPhone12, 10);
            warehouse.Delive(iPhone11, 1);

            warehouse.Inventory.ShowGoods();

            Cart cart = shop.Cart();
            cart.Add(iPhone12, 4);
            cart.Add(iPhone11, 3);

            cart.Inventory.ShowGoods();

            Console.WriteLine(cart.Order().PayLink);

            cart.Add(iPhone12, 9);
        }
    }

    class Shop
    {
        private readonly Warehouse _warehouse;

        public Shop(Warehouse warehouse)
        {
            _warehouse = warehouse;
        }

        public bool IsHaveGood(string goodName, out int count)
        {
            return _warehouse.Inventory.IsHaveGood(goodName, out count);
        }

        public Cart Cart()
        {
            return new Cart(_warehouse);
        }
    }

    class Good
    {
        public Good(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    class Cart
    {
        private readonly Inventory _inventory;
        private readonly IDeliveOutOnly _warehouse;

        public Cart(IDeliveOutOnly warehouse)
        {
            _inventory = new();
            _warehouse = warehouse;
        }

        public IReadOnlyInventory Inventory => _inventory;

        public void Add(Good good, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            _warehouse.Inventory.IsHaveGood(good.Name, out int havedCount);
            _inventory.IsHaveGood(good.Name, out int countInOrder);

            if (havedCount < count + countInOrder)
            {
                throw new ArgumentOutOfRangeException("Не возможно добавить весь товар в корзину. На складе не хватает товара.");
            }

            for (int i = 0; i < count; i++)
            {
                _inventory.TakeGood(good);
            }
        }

        public List<Good> GiveAllGoods()
        {
            return _inventory.GiveAllGoods();
        }

        public Order Order()
        {
            List<Good> goodsInCart = GiveAllGoods();
            List<Good> goodsForOrder = new();
            Random random = new();
            string payLink = random.Next(int.MaxValue).ToString();

            foreach (var good in goodsInCart)
            {
                goodsForOrder.Add(_warehouse.DeliveOut(good.Name));
            }

            return new Order(goodsForOrder, payLink);
        }
    }

    class Order
    {
        private readonly Inventory _inventory;

        public Order(List<Good> goods, string payLink)
        {
            _inventory = new();
            PayLink = payLink;
            AddRange(goods);
        }

        public string PayLink { get; }

        public IReadOnlyInventory Inventory => _inventory;

        private void Add(Good good)
        {
            _inventory.TakeGood(good);
        }

        private void AddRange(List<Good> goods)
        {
            foreach (var good in goods)
            {
                Add(good);
            }
        }
    }

    class Warehouse : IDeliveOutOnly
    {
        private readonly Inventory _inventory;

        public Warehouse()
        {
            _inventory = new();
        }

        public IReadOnlyInventory Inventory => _inventory;

        public void Delive(Good good, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);

            for (int i = 0; i < count; i++)
            {
                Delive(good);
            }
        }

        public void Delive(Good good)
        {
            _inventory.TakeGood(good);
        }

        public Good DeliveOut(string goodName)
        {
            return _inventory.GiveGood(goodName);
        }
    }

    class Slot
    {
        private readonly Good _good;

        public Slot(Good good)
        {
            _good = good;
        }

        public string GoodName => _good.Name;

        public int Count { get; private set; }

        private Good GoodClone => new Good(GoodName);

        public void Take(Good good)
        {
            if (good.Name == _good.Name)
            {
                Count++;
            }
            else
            {
                throw new ArgumentException();
            }
        }

        public Good Give()
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(Count);
            Count--;
            return GoodClone;
        }
    }

    class Inventory : IReadOnlyInventory
    {
        private readonly List<Slot> _goodSlots;

        public Inventory()
        {
            _goodSlots = new();
        }

        public bool IsHaveGood(string goodName, out int count)
        {
            bool isHaveSlot = TryGetSlot(goodName, out Slot returnedSlot);
            bool isHaveGood = false;
            count = 0;

            if (isHaveSlot)
            {
                count = returnedSlot.Count;
                isHaveGood = count > 0;
            }

            return isHaveGood;
        }

        public void TakeGood(Good good)
        {
            Slot slot;

            if (TryGetSlot(good.Name, out Slot returnedSlot))
            {
                slot = returnedSlot;
            }
            else
            {
                slot = new Slot(good);
                _goodSlots.Add(slot);
            }

            slot.Take(good);
        }

        public void ShowGoods()
        {
            foreach (var slot in _goodSlots)
            {
                Console.WriteLine(slot.GoodName + " - " + slot.Count);
            }
        }

        public List<Good> GiveAllGoods()
        {
            List<Good> goods = new();

            foreach (var slot in _goodSlots)
            {
                while (slot.Count > 0)
                {
                    goods.Add(slot.Give());
                }
            }

            return goods;
        }

        public Good GiveGood(string goodName)
        {
            Good returnedGood = null;

            if (TryGetSlot(goodName, out Slot returnedSlot))
            {
                if (returnedSlot.Count > 0)
                {
                    returnedGood = returnedSlot.Give();
                }
            }

            if (returnedGood == null)
            {
                throw new ArgumentException();
            }

            return returnedGood;
        }

        private bool TryGetSlot(string GoodName, out Slot returnedSlot)
        {
            const int FirstIndex = 0;

            Slot[] slotsOfGood = _goodSlots.Where(currentGood => currentGood.GoodName == GoodName).ToArray();
            returnedSlot = null;

            if (slotsOfGood.Length == 0)
            {
                return false;
            }
            else
            {
                returnedSlot = slotsOfGood[FirstIndex];
                return true;
            }
        }
    }

    interface IReadOnlyInventory
    {
        void ShowGoods();

        bool IsHaveGood(string goodName, out int count);
    }

    interface IDeliveOutOnly
    {
        public IReadOnlyInventory Inventory { get; }

        Good DeliveOut(string goodName);
    }
}
