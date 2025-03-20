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

            Console.WriteLine(shop.MakeOrder(cart).PayLink);

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
            return new Cart(this);
        }

        public Order MakeOrder(Cart cart)
        {
            List<Good> goodsInCart = cart.GiveAllGoods();
            List<Good> goodsForOrder = new();
            Random random = new();
            string payLink = random.Next(int.MaxValue).ToString();
            bool isSucces = true;

            foreach (var good in goodsInCart)
            {
                if (_warehouse.TryDeliveOut(good.Name, out Good returnedGood))
                {
                    goodsForOrder.Add(returnedGood);
                }
                else
                {
                    isSucces = false;
                }
            }

            if (isSucces)
            {
                return new Order(goodsForOrder, payLink);
            }
            else
            {
                foreach (var good in goodsForOrder)
                {
                    _warehouse.Delive(good);
                }

                Console.WriteLine("Не получилось создать заказ. На складе не хватает товара.");
                return null;
            }
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
        private readonly Shop _shop;
        private readonly Inventory _inventory;

        public Cart(Shop shop)
        {
            _shop = shop;
            _inventory = new();
        }

        public IReadOnlyInventory Inventory => _inventory;

        public void Add(Good good, int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            _shop.IsHaveGood(good.Name, out int havedCount);
            _inventory.IsHaveGood(good.Name, out int countInOrder);

            if (havedCount >= count + countInOrder)
            {
                for (int i = 0; i < count; i++)
                {
                    _inventory.TakeGood(good);
                }
            }
            else
            {
                Console.WriteLine("Не получилось добавить товар в корзину. На складе не хватает товара.");
            }
        }

        public List<Good> GiveAllGoods()
        {
            return _inventory.GiveAllGoods();
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

    class Warehouse
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

        public bool TryDeliveOut(string goodName, out Good good)
        {
            return _inventory.TryGiveGood(goodName, out good);
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

        public bool TryGiveGood(string goodName, out Good returnedGood)
        {
            returnedGood = null;

            if (TryGetSlot(goodName, out Slot returnedSlot))
            {
                if (returnedSlot.Count > 0)
                {
                    returnedGood = returnedSlot.Give();
                }
            }

            return returnedGood == null == false;
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
}
