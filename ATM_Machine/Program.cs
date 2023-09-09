using Microsoft.VisualBasic;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Runtime;

class Program {

    public static event Action OnDisplayBalance;
    public static event Action OnCardNumberEntered;
    public static event Action OnPinNumberEntered;

    public static event decimalAction OnDeposit;
    public static event decimalAction OnWithdrawal;

    public delegate void decimalAction(decimal amount);

    static void Main() {

        ATM atm = new ATM();

        Console.Write("Enter a card number: ");
        ulong cardChoice = Convert.ToUInt64(Console.ReadLine());

        atm.CardNumber = cardChoice;

        OnCardNumberEntered();

        if (!atm.isRunning)
            goto PASS;

        Console.Write("\nEnter a pin: ");
        uint pinChoice = Convert.ToUInt32(Console.ReadLine());

        atm.Pin = pinChoice;

        OnPinNumberEntered();

        while (atm.isRunning) {

            Console.WriteLine();
            Console.WriteLine("1. Check Balance");
            Console.WriteLine("2. Withdrawal");
            Console.WriteLine("3. Deposit");
            Console.WriteLine("4. See all previous transactions");
            Console.WriteLine("5. Exit\n");

            int userChoice = Int32.Parse(Console.ReadLine());

            switch (userChoice) {

                case 1:
                    OnDisplayBalance();
                break;

                case 2:

                    Console.Write("\nEnter the withdrawal amount: ");
                    decimal withdrawalAmount = Convert.ToDecimal(Console.ReadLine());

                    if (atm.numOfTransactions >= atm.MaxTransactionAmount) {

                        Console.WriteLine($"You've reached a maximum of {atm.MaxTransactionAmount} transactions.");
                        atm.Exit();
                        break;

                    }

                    OnWithdrawal(withdrawalAmount);

                break;

                case 3:

                    Console.Write("\nEnter the deposit amount: ");
                    decimal depositAmount = Convert.ToDecimal(Console.ReadLine());

                    if (atm.numOfTransactions >= atm.MaxTransactionAmount) {

                        Console.WriteLine($"You've reached a maximum of {atm.MaxTransactionAmount} transactions.");
                        atm.Exit();
                        break;

                    }

                    OnDeposit(depositAmount);

                break;

                case 4:

                    Console.WriteLine("TYPE\tAMOUNT");
                    Console.WriteLine("------------");

                    int counter = 0;

                    for (int i = atm.TransactionTypes.Count - 1; i >= 0; i--) {

                        if (counter > 5)
                            break;

                        Console.WriteLine(atm.TransactionTypes[i] + "\t$" + atm.TransactionAmounts[i]);
                        counter++;

                    }

                break;

                case 5: atm.isRunning = false; break;

                default:

                    Console.WriteLine("Invalid option. Please try again.");

                break;

            }

        }

    PASS:
        if (!atm.isRunning)
            atm.Dispose();

    }

}

class ATM : IDisposable {

    const decimal MAX_AMOUNT = 1000;
    readonly int cardNumberDigitAmount = 8;
    readonly int pinNumberDigitAmount = 4;
    readonly int maxTransactionAmount = 10;

    public int MaxTransactionAmount { get => maxTransactionAmount; }

    List<string> _transactionTypes = new List<string>();
    List<decimal> _transactionAmounts = new List<decimal>();

    public List<string>    TransactionTypes { get => _transactionTypes;   }
    public List<decimal> TransactionAmounts { get => _transactionAmounts; }
  
    bool _isRunning = true;
    public bool isRunning { get => _isRunning; set => _isRunning = value; }

    decimal _balance;
    ulong _pin, _cardNumber;

    uint _numOfTransactions;

    public decimal Balance { get => _balance; }

    public ulong Pin { get => _pin; set => _pin = value; }
    public ulong CardNumber { get => _cardNumber; set => _cardNumber = value; }

    public uint numOfTransactions { get => _numOfTransactions; }

    public ATM() {
    
        Console.WriteLine("ATM");
        Console.WriteLine("---");

        this._balance = 0;
        this._pin = 0;
        this._cardNumber = 00000000;
        this._numOfTransactions = 0;

        Program.OnDisplayBalance += OutputBalance;
        Program.OnDeposit += Deposit;
        Program.OnDeposit += OutputNewBalance;
        Program.OnWithdrawal += WithDrawal;
        Program.OnWithdrawal += OutputNewBalance;

        Program.OnCardNumberEntered += ValidateCardNumber;
        Program.OnPinNumberEntered += ValidatePinNumber;

    }

    public void ValidateCardNumber() {

        string cardNumberToString = CardNumber.ToString();

        if (cardNumberToString.Length != cardNumberDigitAmount) {

            Console.WriteLine($"Error: Card number must have {cardNumberDigitAmount} digits only.");
            Exit();

        }

    }

    public void ValidatePinNumber() {

        string pinNumberToString = Pin.ToString();

        if (pinNumberToString.Length != pinNumberDigitAmount && pinNumberToString.Length != 0) {

            Console.WriteLine($"Error: Pin number must have {pinNumberDigitAmount} digits only.");
            Exit();

        }

    }

    public void Deposit(decimal amount) {

        if (amount > MAX_AMOUNT) {

            amount = MAX_AMOUNT;

            Console.WriteLine($"Error: Too much money deposited. ${MAX_AMOUNT} deposited instead.");

        }

        this._balance += amount;
        this._numOfTransactions++;

        _transactionAmounts.Add(amount);
        _transactionTypes.Add("Deposit");

    }

 
    public void WithDrawal(decimal amount) {

        if (amount > MAX_AMOUNT)
            amount = MAX_AMOUNT;

        this._balance -= amount;
        this._numOfTransactions++;

        if (Balance < 0) {

            Console.WriteLine("Error: Too much to withdrawal, your balance is now zero.");
            this._balance = 0;

        }

        _transactionAmounts.Add(amount);
        _transactionTypes.Add("Withdrawal");

    }

    public void OutputBalance() => Console.WriteLine($"\nYour current balance is ${Balance}.");
    public void OutputNewBalance(decimal amount) => Console.WriteLine($"\nYour new balance is ${Balance}.");

    public void Exit() => this.isRunning = false;

    public void Dispose() => GC.SuppressFinalize(this);

    //I hate CSharp, C++ actually lets you control what is allocated on stack and heap (although I do admit that would lead to the risk of memory leaks lol)
    ~ATM() {

        Console.WriteLine("Thank you for using this ATM. Have a nice day.");

        Program.OnDisplayBalance -= OutputBalance;
        Program.OnDeposit -= Deposit;
        Program.OnDeposit -= OutputNewBalance;
        Program.OnWithdrawal -= WithDrawal;
        Program.OnWithdrawal -= OutputNewBalance;

        Program.OnCardNumberEntered -= ValidateCardNumber;
        Program.OnPinNumberEntered -= ValidatePinNumber;

    }

}