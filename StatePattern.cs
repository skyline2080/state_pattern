using System;
using System.Runtime.CompilerServices;

var t1 = System.Diagnostics.Stopwatch.StartNew();

var person = new Person();

person.SayHi();
person.SayBye();
person.SayHi();

person.ResetState();

person.SayBye();
person.SayHi();

Console.WriteLine(t1.ElapsedTicks);

public class Person { 
    public string Name {get; set;} = "Joe";
    State state; 

    public Person() => ResetState();
    
    public void SayHi() => state.SayHi(this);
    public void SayBye() => state.SayBye(this);
    public override string ToString() => Name;
    
    internal void ResetState() => state = State.Default();

    // volatile behaviour (aka state) implemented as struct for:
    // behaviour is bound to the object's data quite naturally
    // this solution provide for in-place allocation 
    // no penalties for ptr hops and saved extra bits of memory as opposied for class stub
    // what the below struct does is essentially just bookkeeping of ptrs to corresponding delegates
    struct State {
        public Action<Person> SayHi;  
        public Action<Person> SayBye; 

        public static State Default() {
            var state = new State();
            state.ToFirstMeeting();
            return state;
        }
       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ToAcquainted() {
            SayHi  = ticks[ACQUAINTED][HI];
            SayBye = ticks[ACQUAINTED][BYE];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void ToFirstMeeting() {
            SayHi  = ticks[FIRSTMEETING][HI];
            SayBye = ticks[FIRSTMEETING][BYE];
        }

        static Action<Person>[][] ticks = {
            new Action<Person>[2],
            new Action<Person>[2],
        }; 
        static State() {
            ticks[FIRSTMEETING][HI] = p => { 
                Console.WriteLine("hi, never seen each other before, I'm  " + p); 
                p.state.ToAcquainted(); 
            };

            ticks[FIRSTMEETING][BYE] = p => { 
                Console.WriteLine("bye, never seen each other before, I'm " + p); 
                p.state.ToAcquainted();  
            };

            ticks[ACQUAINTED][HI]  = p => Console.WriteLine("hi, met each other earlier, I'm  " + p);
            ticks[ACQUAINTED][BYE] = p => Console.WriteLine("bye, met each other earlier, I'm " + p); 

        }

        const int FIRSTMEETING = 0, ACQUAINTED = 1;
        const int HI = 0, BYE = 1;
    }
}