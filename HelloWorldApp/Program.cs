// See https://aka.ms/new-console-template for more information

using PM.Scheduler.GTools;
using PM.Scheduler.GTools.Models;
using System.Data;

Console.WriteLine("Hello, World!");

void DemoMaterial()
{
    //We have to produce products (P)
    //We can use different recipes for the same product, for example, ProductA can be produced with two recepits:
    //- mixing 1units of Semielaborated1 with 2 of RawMat1 
    //- mixing 5 units of RawMat1
    //Semielaborated1 is produced with 2xRawMat1+2xRawMat2

    //RECIPE 1 for Product A
    List<Component> bom = new();
    bom.Add(new Component()
    {
        Recipe = 1,
        Parent = "PA",
        SubComponent = "S1",
        Quantity = 1
    });
    bom.Add(new Component()
    {
        Recipe = 1,
        Parent = "PA",
        SubComponent = "R1",
        Quantity = 2
    });
    //RECIPE 2 FOR ProductA
    bom.Add(new Component()
    {
        Recipe = 2,
        Parent = "PA",
        SubComponent = "R1",
        Quantity = 5
    });
    //Recipe for S1
    bom.Add(new Component()
    {
        Recipe = 1,
        Parent = "S1",
        SubComponent = "R1",
        Quantity = 2
    });
    bom.Add(new Component()
    {
        Recipe = 1,
        Parent = "S1",
        SubComponent = "R2",
        Quantity = 2
    });

    //we have a limited stock in factory: we can have stock of products, semielaborated or raw materials. Each stock has an entry/Created date and a Expire date
    List<MaterialStockLine> stock = new List<MaterialStockLine>();
    stock.Add(new() { Reference = "R1", Quantity = 50, CreatedOn = new DateTime(2022, 8, 1), ExpiresOn = new DateTime(2023, 1, 1) });
    stock.Add(new() { Reference = "R1", Quantity = 350, CreatedOn = new DateTime(2022, 12, 12), ExpiresOn = new DateTime(2024, 6, 1) });
    stock.Add(new() { Reference = "R2", Quantity = 75, CreatedOn = new DateTime(2022, 12, 12), ExpiresOn = new DateTime(2024, 6, 1) });
    stock.Add(new() { Reference = "S1", Quantity = 15, CreatedOn = new DateTime(2022, 12, 12), ExpiresOn = new DateTime(2025, 6, 1) });
    stock.Add(new() { Reference = "PA", Quantity = 1, CreatedOn = new DateTime(2022, 12, 12), ExpiresOn = new DateTime(2030, 6, 1) });
    
    //Finally, we have a demand to satisfy
    List<MaterialOrder> orders= new List<MaterialOrder>();
    orders.Add(new MaterialOrder()
    {
        Reference = "PA",
        Quantity = 40,
        Target = new DateTime(2023, 6, 1)
    });


    //Solve: How can I make 40 units of PA???????????
    //I cannot use any component that expires before target date (I cannot use 50 units of R1 in first stock line)
    //Solution1: using Recipe=1
    // I'll need 40 S1-->15S1 from stock+25x2R1+25x2R2 
    // I'l need 2x40 R1
    // Total 15S1, 130xR1, 50xR2
    //
    //Solution2: using Recipe2
    //I'll need 5x40 R1
    //Total 200R1
    
    //Solution1 is better for MAX_STOCK criterium, because use more abundant material R1, maximizing stocks
    //Solution2 is better for MIN_STOCK criterium, because reduces stock level, minimizin stocks
    //Solution1 is better for FIFO criterium, because consumes First In first
    //Solution1 is better for FEFO criterium, because consumes First to be Expired first






}

/*
 * Workshop with three machines. 
 * We have to schedule two operations, with two process each operation.
 * Op1: process1 can be done in Fab01 (takes 10 hours) or Fab02 (takes 20 hours to complete). Process 2 have to be done in Emb01
 * Op2: process1 can be done in Fab01 (takes 15 hours) or Fab02 (takes 12 hours to complete). Process 2 have to be done in Emb01
 * Additional consideration: Subproducts of process1 cannot be waiting more than 1 hour to be completed in process2 (MaxWaitTime=1)
 * 
 * Solution: 
 *  Resource Fab01:: | op1_1  |
 *  Resource Emb01:: **********|op1_2|p2_2|
 *  Resource Fab02:: **|  op2_1   |
 *  
 *  Fab1 starts producing 1.1, when it finished starts 1.2 in Emb01
 *  Fab2 delay to start with 2.1, because if starts in time 0, when it finish there won't be available machine to make process 2.2
 *  
 */
void RunDemo1()
{

    //1. Definimos recursos
    Resource fabUno = new Resource("Fab01");
    Resource fabDos = new Resource("Fab02");
    Resource embotelladora = new Resource("Emb01");

    //2. Definimos operaciones con sus procesos
    //2.1 Op1
    Process op1_1 = new Process("op1_1", "Fabricación");
    op1_1.AddAllowedResource(fabUno, 10);
    op1_1.AddAllowedResource(fabDos, 20);
    op1_1.MaxWaitTime = 1;
    Process op1_2 = new Process("op1_2", "Embotellado");
    op1_2.AddAllowedResource(embotelladora, 5);

    Operation op1 = new("Op01", "Operación 1");
    op1.AddProcess(op1_1, op1_2);

    //2.2 Op2
    Process op2_1 = new Process("op2_1", "Fabricación");
    op2_1.AddAllowedResource(fabUno, 15);
    op2_1.AddAllowedResource(fabDos, 12);
    op2_1.MaxWaitTime = 1;

    Process op2_2 = new Process("op2_2", "Embotellado");
    op2_2.AddAllowedResource(embotelladora, 5);

    Operation op2 = new("Op02", "Operación 2");
    op2.AddProcess(op2_1, op2_2);

    //3. Calculos
    Engine engine = new();
    engine.AddOperations(op1, op2);

    engine.Calculate();

    foreach (var item in engine.Scheduling.Keys)
    {
        Utils.Print(item.Code, engine.Scheduling[item]);
    }
}

/*
 * Demo 2 is a simpler workshop. There are 4 single process operations. 
 * There are two resources (machines), every process can be done in resource Fab01 or Fab02, with different times to finish
 * Solution:
 *  Resource Fab01:: | op4_1  || op3_1  || op1_1  |
 *  Resource Fab02:: |     op2_1      |
 *  
 *  op 2 is done in fab02, anothers operation choose faster resource Fab01
 */
void RunDemo2()
{

    //1. Definimos recursos
    Resource fabUno = new Resource("Fab01");
    Resource fabDos = new Resource("Fab02");

    //2. Definimos operaciones con sus procesos
    //2.1 Op1
    Process op1_1 = new Process("op1_1", "Fabricación");
    op1_1.AddAllowedResource(fabUno, 10);
    op1_1.AddAllowedResource(fabDos, 16);
    op1_1.MaxWaitTime = 1;
    Operation op1 = new("Op01", "Operación 1");
    op1.AddProcess(op1_1);

    //2.1 Op1
    Process op2_1 = new Process("op2_1", "Fabricación");
    op2_1.AddAllowedResource(fabUno, 15);
    op2_1.AddAllowedResource(fabDos, 18);
    op2_1.MaxWaitTime = 1;
    Operation op2 = new("Op02", "Operación 2");
    op2.AddProcess(op2_1);

    //2.1 Op1
    Process op3_1 = new Process("op3_1", "Fabricación");
    op3_1.AddAllowedResource(fabUno, 10);
    op3_1.AddAllowedResource(fabDos, 16);
    op3_1.MaxWaitTime = 1;
    Operation op3 = new("Op03", "Operación 3");
    op3.AddProcess(op3_1);

    //2.1 Op1
    Process op4_1 = new Process("op4_1", "Fabricación");
    op4_1.AddAllowedResource(fabUno, 10);
    op4_1.AddAllowedResource(fabDos, 16);
    op4_1.MaxWaitTime = 1;
    Operation op4 = new("Op04", "Operación 4");
    op4.AddProcess(op4_1);

    //3. Calculos
    Engine engine = new();
    engine.AddOperations(op1, op2, op3, op4);

    engine.Calculate();

    foreach (var item in engine.Scheduling.Keys)
    {
        Utils.Print(item.Code, engine.Scheduling[item]);
    }
}

/*
 * Demo 3 is the problem to be solved
 * Is similar to Demo2, but now, we have different ProcessTypes
 * Each time a machine change to a new process, it has a "setup time" to be prepared to the new process
 * Setup time depends of ProcessType previous and new
 */
void RunDemo3()
{
    //TODO: Introduce setup time from this array
    Dictionary<KeyValuePair<int, int>, int> setupArray = new();
    setupArray.Add(new(0, 0), 0);
    setupArray.Add(new(0, 1), 5);
    setupArray.Add(new(1, 0), 5);
    setupArray.Add(new(0, 0), 0);

    //1. Definimos recursos
    Resource fabUno = new Resource("Fab01");
    Resource fabDos = new Resource("Fab02");

    //2. Definimos operaciones con sus procesos
    //2.1 Op1
    Process op1_1 = new Process("op1_1", "Fabricación");
    op1_1.AddAllowedResource(fabUno, 10);
    op1_1.AddAllowedResource(fabDos, 12);
    op1_1.MaxWaitTime = 1;
    op1_1.ProcessType = 0;
    Operation op1 = new("Op01", "Operación 1");
    op1.AddProcess(op1_1);

    //2.1 Op1
    Process op2_1 = new Process("op2_1", "Fabricación");
    op2_1.AddAllowedResource(fabUno, 15);
    op2_1.AddAllowedResource(fabDos, 18);
    op2_1.MaxWaitTime = 1;
    op2_1.ProcessType = 1;
    Operation op2 = new("Op02", "Operación 2");
    op2.AddProcess(op2_1);

    //2.1 Op1
    Process op3_1 = new Process("op3_1", "Fabricación");
    op3_1.AddAllowedResource(fabUno, 10);
    op3_1.AddAllowedResource(fabDos, 12);
    op3_1.MaxWaitTime = 1;
    op3_1.ProcessType = 1;
    Operation op3 = new("Op03", "Operación 3");
    op3.AddProcess(op3_1);

    //2.1 Op1
    Process op4_1 = new Process("op4_1", "Fabricación");
    op4_1.AddAllowedResource(fabUno, 10);
    op4_1.AddAllowedResource(fabDos, 12);
    op4_1.MaxWaitTime = 1;
    op4_1.ProcessType = 1;
    Operation op4 = new("Op04", "Operación 4");
    op4.AddProcess(op4_1);

    //3. Calculos
    Engine engine = new();
    engine.AddOperations(op1, op2, op3, op4);

    engine.Calculate(setupArray);

    foreach (var item in engine.Scheduling.Keys)
    {
        Utils.Print(item.Code, engine.Scheduling[item]);
    }
}

void RunDemo4()
{
    //TODO: Introduce setup time from this array
    Dictionary<KeyValuePair<int, int>, int> setupArray = new();
    setupArray.Add(new(0, 0), 0);
    setupArray.Add(new(0, 1), 5);
    setupArray.Add(new(1, 0), 5);
    setupArray.Add(new(0, 0), 0);

    //1. Definimos recursos
    Resource fabUno = new Resource("Fab01");
    Resource fabDos = new Resource("Fab02");

    //2. Definimos operaciones con sus procesos
    //2.1 Op1
    Process op1_1 = new Process("op1_1", "Fabricación");
    op1_1.AddAllowedResource(fabUno, 12);
    op1_1.AddAllowedResource(fabDos, 13);
    op1_1.MaxWaitTime = 1;
    op1_1.ProcessType = 2;
    Operation op1 = new("Op01", "Operación 1");
    op1.AddProcess(op1_1);

    //2.1 Op1
    Process op2_1 = new Process("op2_1", "Fabricación");
    op2_1.AddAllowedResource(fabUno, 15);
    op2_1.AddAllowedResource(fabDos, 13);
    op2_1.MaxWaitTime = 1;
    op2_1.ProcessType = 1;
    Operation op2 = new("Op02", "Operación 2");
    op2.AddProcess(op2_1);

    //2.1 Op1
    Process op3_1 = new Process("op3_1", "Fabricación");
    op3_1.AddAllowedResource(fabUno, 16);
    op3_1.AddAllowedResource(fabDos, 13);
    op3_1.MaxWaitTime = 1;
    op3_1.ProcessType = 0;
    Operation op3 = new("Op03", "Operación 3");
    op3.AddProcess(op3_1);

    //2.1 Op1
    Process op4_1 = new Process("op4_1", "Fabricación");
    op4_1.AddAllowedResource(fabUno, 12);
    op4_1.AddAllowedResource(fabDos, 16);
    op4_1.MaxWaitTime = 1;
    op4_1.ProcessType = 3;
    Operation op4 = new("Op04", "Operación 4");
    op4.AddProcess(op4_1);

    Process op5_1 = new Process("op5_1", "Fabricación");
    op5_1.AddAllowedResource(fabUno, 18);
    op5_1.AddAllowedResource(fabDos, 19);
    op5_1.MaxWaitTime = 1;
    op5_1.ProcessType = 2;
    Operation op5 = new("Op05", "Operación 5");
    op5.AddProcess(op5_1);

    //3. Calculos
    Engine engine = new();
    engine.AddOperations(op1, op2, op3, op4, op5);

    engine.Calculate(setupArray);

    foreach (var item in engine.Scheduling.Keys)
    {
        Utils.Print(item.Code, engine.Scheduling[item]);
    }
}