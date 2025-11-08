using System;
using System.Collections.Generic;
using System.Linq;

// --- Creational Pattern: Abstract Factory ---
// Purpose: Provide an interface for creating families of related or dependent objects
//          without specifying their concrete classes.

#region Creational_AbstractFactory

/// <summary>
/// Represents a product that can be part of an order. Base interface for all product types.
/// </summary>
public interface IProduct
{
    string Name { get; }
    decimal Price { get; }
    void Display();
}

/// <summary>
/// Represents an abstract order.
/// </summary>
public interface IOrder
{
    Guid OrderId { get; }
    List<IProduct> Items { get; }
    decimal TotalAmount { get; }
    void AddItem(IProduct product);
    void DisplayOrderDetails();
}

/// <summary>
/// The Abstract Factory interface for creating families of products and orders.
/// E.g., a factory for Digital products and Digital orders, or Physical products and Physical orders.
/// </summary>
public interface IOrderFactory
{
    IProduct CreateDigitalProduct(string name, decimal price, string downloadUrl);
    IProduct CreatePhysicalProduct(string name, decimal price, double weight);
    IOrder CreateOrder();
}

// Concrete Products for Digital Category
public class DigitalProduct : IProduct
{
    public string Name { get; }
    public decimal Price { get; }
    public string DownloadUrl { get; }

    public DigitalProduct(string name, decimal price, string downloadUrl)
    {
        Name = name;
        Price = price;
        DownloadUrl = downloadUrl;
    }

    public void Display()
    {
        Console.WriteLine($"  Digital Product: {Name}, Price: {Price:C}, Download: {DownloadUrl}");
    }
}

// Concrete Products for Physical Category
public class PhysicalProduct : IProduct
{
    public string Name { get; }
    public decimal Price { get; }
    public double Weight { get; }

    public PhysicalProduct(string name, decimal price, double weight)
    {
        Name = name;
        Price = price;
        Weight = weight;
    }

    public void Display()
    {
        Console.WriteLine($"  Physical Product: {Name}, Price: {Price:C}, Weight: {Weight}kg");
    }
}

// Concrete Order implementation for both Digital and Physical items.
// In a more complex scenario, you might have specific DigitalOrder or PhysicalOrder.
public class CustomerOrder : IOrder
{
    public Guid OrderId { get; } = Guid.NewGuid();
    public List<IProduct> Items { get; } = new List<IProduct>();
    public decimal TotalAmount => Items.Sum(item => item.Price);

    public void AddItem(IProduct product)
    {
        Items.Add(product);
    }

    public void DisplayOrderDetails()
    {
        Console.WriteLine($"Order ID: {OrderId}");
        Console.WriteLine("Items:");
        foreach (var item in Items)
        {
            item.Display();
        }
        Console.WriteLine($"Total: {TotalAmount:C}");
    }
}

/// <summary>
/// Concrete Factory for creating a family of digital-focused products and a generic order.
/// </summary>
public class DigitalOrderFactory : IOrderFactory
{
    public IProduct CreateDigitalProduct(string name, decimal price, string downloadUrl)
    {
        return new DigitalProduct(name, price, downloadUrl);
    }

    public IProduct CreatePhysicalProduct(string name, decimal price, double weight)
    {
        // This factory specializes in digital, so it might not create physical products,
        // or it might return a default/throw an exception. For simplicity, we'll create a generic one.
        Console.WriteLine("Warning: DigitalOrderFactory creating a PhysicalProduct.");
        return new PhysicalProduct(name, price, weight);
    }

    public IOrder CreateOrder()
    {
        return new CustomerOrder();
    }
}

/// <summary>
/// Concrete Factory for creating a family of physical-focused products and a generic order.
/// </summary>
public class PhysicalOrderFactory : IOrderFactory
{
    public IProduct CreateDigitalProduct(string name, decimal price, string downloadUrl)
    {
        // This factory specializes in physical, same logic as above.
        Console.WriteLine("Warning: PhysicalOrderFactory creating a DigitalProduct.");
        return new DigitalProduct(name, price, downloadUrl);
    }

    public IProduct CreatePhysicalProduct(string name, decimal price, double weight)
    {
        return new PhysicalProduct(name, price, weight);
    }

    public IOrder CreateOrder()
    {
        return new CustomerOrder();
    }
}

#endregion

// --- Structural Pattern: Adapter ---
// Purpose: Convert the interface of a class into another interface clients expect.
//          Adapter lets classes work together that couldn't otherwise because of incompatible interfaces.

#region Structural_Adapter

/// <summary>
/// Our modern payment gateway expects this interface.
/// </summary>
public interface IPaymentGateway
{
    bool ProcessPayment(decimal amount, string cardNumber, string expiryDate, string cvv);
    bool RefundPayment(string transactionId);
}

/// <summary>
/// An "old" or "third-party" legacy payment system with an incompatible interface.
/// We cannot change this interface.
/// </summary>
public class LegacyPaymentProcessor
{
    public bool MakePayment(decimal value, string creditCardNumber, int expMonth, int expYear)
    {
        Console.WriteLine($"  Legacy system processing payment of {value:C} from card {creditCardNumber}...");
        // Simulate processing...
        return true; // Always succeeds for demo
    }

    public string GetTransactionStatus(string transactionRef)
    {
        // Not directly used by our IPaymentGateway, but part of the legacy system.
        return "Completed";
    }
}

/// <summary>
/// The Adapter that makes the LegacyPaymentProcessor compatible with our IPaymentGateway.
/// </summary>
public class LegacyPaymentAdapter : IPaymentGateway
{
    private readonly LegacyPaymentProcessor _legacyProcessor;

    public LegacyPaymentAdapter(LegacyPaymentProcessor legacyProcessor)
    {
        _legacyProcessor = legacyProcessor;
    }

    public bool ProcessPayment(decimal amount, string cardNumber, string expiryDate, string cvv)
    {
        // Parse expiryDate from "MM/YY" format to separate month and year for the legacy system.
        var parts = expiryDate.Split('/');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int month) || !int.TryParse(parts[1], out int year))
        {
            Console.WriteLine("  Error: Invalid expiry date format for legacy system.");
            return false;
        }

        // Assume year is "25" for "2025", converting to full year.
        int fullYear = 2000 + year;

        Console.WriteLine("  Using LegacyPaymentAdapter...");
        return _legacyProcessor.MakePayment(amount, cardNumber, month, fullYear);
    }

    public bool RefundPayment(string transactionId)
    {
        // The legacy processor might not have a direct refund method exposed
        // or it might require a different business process.
        Console.WriteLine($"  Warning: Refund not directly supported by LegacyPaymentAdapter for transaction {transactionId}. Manual intervention might be required.");
        return false; // Simulate failure or unsupported operation
    }
}

#endregion

// --- Behavioral Pattern: Command ---
// Purpose: Encapsulate a request as an object, thereby letting you parameterize clients with different requests,
//          queue or log requests, and support undoable operations.

#region Behavioral_Command

/// <summary>
/// The Command interface declares a method for executing a command.
/// </summary>
public interface ICommand
{
    void Execute();
    void Undo(); // For commands that support undo
}

/// <summary>
/// Receiver: The class that performs the actual operation.
/// In our case, the OrderProcessor and PaymentProcessor will be receivers.
/// </summary>
public class OrderProcessor
{
    public void PlaceOrder(IOrder order)
    {
        Console.WriteLine($"  OrderProcessor: Placing order {order.OrderId} with {order.TotalAmount:C}...");
        order.DisplayOrderDetails();
        // Logic to persist order, update inventory etc.
    }

    public void CancelOrder(IOrder order)
    {
        Console.WriteLine($"  OrderProcessor: Cancelling order {order.OrderId}.");
        // Logic to revert order status, restock items etc.
    }
}

/// <summary>
/// Receiver: Handles actual payment processing using an IPaymentGateway.
/// </summary>
public class PaymentSystem
{
    private readonly IPaymentGateway _paymentGateway;

    public PaymentSystem(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway;
    }

    public bool AuthorizePayment(decimal amount, string cardNumber, string expiryDate, string cvv)
    {
        Console.WriteLine($"  PaymentSystem: Attempting to authorize {amount:C}...");
        return _paymentGateway.ProcessPayment(amount, cardNumber, expiryDate, cvv);
    }

    public void RollbackPayment(string transactionId)
    {
        Console.WriteLine($"  PaymentSystem: Attempting to rollback payment {transactionId}.");
        _paymentGateway.RefundPayment(transactionId);
    }
}


/// <summary>
/// Concrete Command: Places an order.
/// </summary>
public class PlaceOrderCommand : ICommand
{
    private readonly OrderProcessor _processor;
    private readonly IOrder _order;
    private bool _isExecuted; // To track state for Undo

    public PlaceOrderCommand(OrderProcessor processor, IOrder order)
    {
        _processor = processor;
        _order = order;
    }

    public void Execute()
    {
        Console.WriteLine("Executing PlaceOrderCommand:");
        _processor.PlaceOrder(_order);
        _isExecuted = true;
    }

    public void Undo()
    {
        if (_isExecuted)
        {
            Console.WriteLine("Undoing PlaceOrderCommand:");
            _processor.CancelOrder(_order);
            _isExecuted = false;
        }
        else
        {
            Console.WriteLine("PlaceOrderCommand was not executed, cannot undo.");
        }
    }
}

/// <summary>
/// Concrete Command: Processes a payment.
/// </summary>
public class ProcessPaymentCommand : ICommand
{
    private readonly PaymentSystem _paymentSystem;
    private readonly decimal _amount;
    private readonly string _cardNumber;
    private readonly string _expiryDate;
    private readonly string _cvv;
    private bool _paymentSuccessful;
    private string _transactionId; // In a real system, gateway would return this

    public ProcessPaymentCommand(PaymentSystem paymentSystem, decimal amount, string cardNumber, string expiryDate, string cvv)
    {
        _paymentSystem = paymentSystem;
        _amount = amount;
        _cardNumber = cardNumber;
        _expiryDate = expiryDate;
        _cvv = cvv;
    }

    public void Execute()
    {
        Console.WriteLine("Executing ProcessPaymentCommand:");
        _paymentSuccessful = _paymentSystem.AuthorizePayment(_amount, _cardNumber, _expiryDate, _cvv);
        if (_paymentSuccessful)
        {
            _transactionId = Guid.NewGuid().ToString(); // Simulate transaction ID
            Console.WriteLine($"  Payment successful! Transaction ID: {_transactionId}");
        }
        else
        {
            Console.WriteLine("  Payment failed.");
        }
    }

    public void Undo()
    {
        if (_paymentSuccessful && !string.IsNullOrEmpty(_transactionId))
        {
            Console.WriteLine("Undoing ProcessPaymentCommand:");
            _paymentSystem.RollbackPayment(_transactionId);
        }
        else if (!_paymentSuccessful)
        {
            Console.WriteLine("Payment was not successful, no need to undo.");
        }
        else
        {
            Console.WriteLine("Payment command was not executed or transaction ID is missing, cannot undo.");
        }
    }
}

/// <summary>
/// Invoker: Stores and executes commands. Can also manage a history of commands for undo/redo.
/// </summary>
public class CommandInvoker
{
    private readonly Stack<ICommand> _history = new Stack<ICommand>();

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _history.Push(command); // Add to history for potential Undo
    }

    public void UndoLastCommand()
    {
        if (_history.Any())
        {
            ICommand lastCommand = _history.Pop();
            lastCommand.Undo();
        }
        else
        {
            Console.WriteLine("No commands to undo.");
        }
    }
}

#endregion

// --- Client Code: Demonstrating the Integration ---

public class ECommerceClient
{
    public static void Main(string[] args)
    {
        Console.WriteLine("--- Setting up the System ---");

        // Abstract Factory Setup
        IOrderFactory digitalFactory = new DigitalOrderFactory();
        IOrderFactory physicalFactory = new PhysicalOrderFactory();

        // Adapter Setup
        LegacyPaymentProcessor legacyProcessor = new LegacyPaymentProcessor();
        IPaymentGateway adaptedLegacyGateway = new LegacyPaymentAdapter(legacyProcessor);
        // In a real system, you might have a ModernPaymentGateway implementing IPaymentGateway too.
        // IPaymentGateway modernGateway = new ModernPaymentGateway();

        // Command Setup (Receivers)
        OrderProcessor orderProcessor = new OrderProcessor();
        PaymentSystem paymentSystem = new PaymentSystem(adaptedLegacyGateway); // Using the adapted gateway
        CommandInvoker invoker = new CommandInvoker();

        Console.WriteLine("\n--- Scenario 1: Creating a Digital Order and Processing Payment ---");
        // Create products and order using the Digital Factory
        IProduct ebook = digitalFactory.CreateDigitalProduct("Clean Code eBook", 29.99m, "http://downloads.example.com/clean-code.pdf");
        IProduct onlineCourse = digitalFactory.CreateDigitalProduct("Design Patterns Course", 199.99m, "http://courses.example.com/dp");
        IOrder digitalOrder = digitalFactory.CreateOrder();
        digitalOrder.AddItem(ebook);
        digitalOrder.AddItem(onlineCourse);

        // Commands for this order
        ICommand placeDigitalOrder = new PlaceOrderCommand(orderProcessor, digitalOrder);
        ICommand processDigitalPayment = new ProcessPaymentCommand(paymentSystem, digitalOrder.TotalAmount, "1234-5678-9012-3456", "12/25", "123");

        invoker.ExecuteCommand(placeDigitalOrder);
        invoker.ExecuteCommand(processDigitalPayment);

        Console.WriteLine("\n--- Scenario 2: Creating a Physical Order and Attempting Refund ---");
        // Create products and order using the Physical Factory
        IProduct tShirt = physicalFactory.CreatePhysicalProduct("Developer T-Shirt", 25.00m, 0.2);
        IProduct mug = physicalFactory.CreatePhysicalProduct("Coffee Mug", 15.00m, 0.5);
        IOrder physicalOrder = physicalFactory.CreateOrder();
        physicalOrder.AddItem(tShirt);
        physicalOrder.AddItem(mug);

        // Commands for this order
        ICommand placePhysicalOrder = new PlaceOrderCommand(orderProcessor, physicalOrder);
        ICommand processPhysicalPayment = new ProcessPaymentCommand(paymentSystem, physicalOrder.TotalAmount, "9876-5432-1098-7654", "01/26", "456");

        invoker.ExecuteCommand(placePhysicalOrder);
        invoker.ExecuteCommand(processPhysicalPayment);

        Console.WriteLine("\n--- Attempting to Undo Last Operations ---");
        invoker.UndoLastCommand(); // Should undo processPhysicalPayment
        invoker.UndoLastCommand(); // Should undo placePhysicalOrder (cancel order)

        Console.WriteLine("\n--- Final State Observation (simplified) ---");
        // In a real system, you'd check DB or service states here.
        Console.WriteLine("System demonstration complete.");
    }
}

