# CScheduler
1. Сайт проекта http://descheduler.com
2. Проект позволяет вызывать указанные функции в смарт контрактах в указанное время
3. Доступ ко всем функциям сайта имеет только зарегистрированный пользователь.

Пример использования:
Для правильного функционирования работы сайта http://cryptopoll.me необходимо проводить завершение раунда. 

После завершения раунда происходит следующее:
- Среди участников распределяется вознаграждение
- Текущий опрос завершается
- Начинается новый опрос

Для реализации завершения раунда в смарт контракте предусмотрен метод executeRound(). Для автоматического вызова этого метода можно использовать данный сервис. Для этого необходимо сделать следующее:
- Перейти на сайт http://descheduler.com и зарегистрироваться
- На главой странице нажать кнопку "Добавить задачу" и заполнить все параметры задачи:
<img src="http://descheduler.com/images/task.png"></img>

</br></br>
# API
<h2>/Api/AddNewTask - добавление новой задачи</h2>
Для программного добавления задач можно использовать API по адресу http://descheduler.com/Api/AddNewTask
Список параметров:
<ul>
<li><b>apiKey</b> (обязательный) - ваш API ключ. При регистрации на сайте http://descheduler.com вам автоматически присваивается уникальный ключ. Найти ключ можно в личном кабинете (Menu - My account - Personal data)</li>
<li><b>name</b> (обязательный) - наименование задачи. Тип строка.</li>
<li><b>network</b> (обязательный) - сеть, в которой находится ваш смарт контракт. Тип строка. Может принимать одно из трех значений: "CreditsNetwork", "DevsDappsTestnet" или "testnet-r4_2".</li>
<li><b>method</b> (обязательный) - публичный метод в смарт контракте, который вы собираетесь вызывать в запланированное время. Тип строка. Например: "executeRound". Указывается без скобок.</li>
<li><b>address</b> (обязательный) - адрес смарт контракта. Тип строка. Например: "GVGAFSYAsTSfnnAZuHzHL43q9UpbvpEZzKn2VmfaMcEH".</li>
<li><b>executionMode</b> (обязательный) - периодичность с которой будет вызываться вышеуказанные метод (method) в смарт контракте. Тип строка. Может принимать одно из трех значений:</li>
    <ul>
    <li><b>Regular</b> - задача будет выполняться регулярно</li>
    <li><b>Once</b> - задача будет выполнена единожды в строго указанное время</li>
    <li><b>CronExpression</b> - выражение в формате Cron. Например: "0,11 0,2,34 0,15 6 APR ? *". Данное выражение можно сформировать автоматически, используя какой-либо онлайн-сервис, например, <a href="https://www.freeformatter.com/cron-expression-generator-quartz.html">freeformatter.com</a></li>
</ul>
</ul>
    
Заполнение других параметров зависит от того какой <b>executionMode</b> используется.</br>
Вариант 1. Если <b>executionMode</b>="Regular", то необходимо передать еще 4 параметра:
    <ul>
    <li><b>regularDateFrom</b> - дата начала выполнения в формате "ММ-ДД-ГГГГ-ЧЧ-ММ-СС". Тип строка.</li>
    <li><b>regularDateTo</b> - дата окончания выполнения в формате "ММ-ДД-ГГГГ-ЧЧ-ММ-СС". Тип строка.</li>
    <li><b>regularPeriod</b> - периодичность. Может принимать 1 из трех значений: "Days", "Hours" или "Minutes". Тип строка.</li>
    <li><b>regularValue</b> - частота выполнения. Тип строка. Целочисленное значение. Например: 1, 3, 5, 10.</li>
    </ul>
Вариант 2. Если <b>executionMode</b>="Once", то необходимо передать еще 1 параметр:
    <ul>
    <li><b>onceDate</b> - дата выполнения в формате "ММ-ДД-ГГГГ-ЧЧ-ММ-СС"</li>
    </ul>
Вариант 3. Если <b>executionMode</b>="CronExpression", то необходимо передать еще 1 параметр:
    <ul>
    <li><b>cronExpression</b> - выражение в формате Cron</li>
    </ul>
    
Пример 1. Метод <b>executeRound</b> в смарт контракте по адресу GVGAFSYAsTSfnnAZuHzHL43q9UpbvpEZzKn2VmfaMcEH будет вызываться с 1-го января по 31-е декабря каждые 3 часа.
http://descheduler.com/Api/AddNewTask?apiKey="YourApiKey"&name="Test1"&network="DevsDappsTestnet"&method="executeRound"&address="GVGAFSYAsTSfnnAZuHzHL43q9UpbvpEZzKn2VmfaMcEH"&executionMode="Regular"&regularDateFrom="01-01-2019-01-01-01"&regularDateTo="12-31-2019-23-59-59"&regularPeriod="Hours"&regularValue="3"

Пример 2. Метод <b>executeRound</b> будет вызван единожды 31 декабря в 23:59:59
http://descheduler.com/Api/AddNewTask?apiKey="YourApiKey"&name="Test2"&network="testnet-r4_2"&method="executeRound"&address="GVGAFSYAsTSfnnAZuHzHL43q9UpbvpEZzKn2VmfaMcEH"&executionMode="Once"&onceDate="12-31-2019-23-59-59"

Пример 3. Метод <b>executeRound</b> будет вызываться согласно расписанию, закодированному в формате cron
http://descheduler.com/Api/AddNewTask?apiKey="YourApiKey"&name="Test3"&network="CreditsNetwork"&method="executeRound"&address="GVGAFSYAsTSfnnAZuHzHL43q9UpbvpEZzKn2VmfaMcEH"&executionMode="CronExpression"&cronExpression="0,11 0,2,34 0,15 6 APR ? *"
</br></br>

<h2>/Api/DeploySmartContract - создать новый смарт контракт</h2>
Для создания нового контракта необходимо отправить post запрос по адресу http://descheduler.com/Api/DeploySmartContract и передать в теле запроса json-объект с 4-мя параметрами:
<ul>
    <li><b>Network</b> - может быть одним из трех значений: 'CreditsNetwork' или 'testnet-r4_2' или 'DevsDappsTestnet'</li>
    <li><b>PublicKey</b> - публичный ключ вашего кошелька</li>
    <li><b>PrivateKey</b> - приватный ключ вашего кошелька</li>
    <li><b>JavaCode</b> - java код вашего смарт контракта</li>    
</ul>
    
В ответе будет возвращен json-объект с 3-мя параметрами:
<ul>
    <li><b>IsSuccess</b> - если смарт контракт добавлен, то значение равно true, иначе false</li>
    <li><b>Address</b> - если смарт контракт добавлен, то значение будет содержать адрес контракта</li> 
    <li><b>Message</b> - если смарт контракт добавлен, то значение равно 'Ok', иначе описание ошибки</li> 
</ul>
    
Пример запроса:    
    
    let model = new Object();            
    model.Network = 'CreditsNetwork'; //CreditsNetwork or testnet-r4_2 or DevsDappsTestnet
    model.PublicKey = '<your public key>';
    model.PrivateKey = '<your private key>';
    model.JavaCode = '' +
        'import com.credits.scapi.annotations.Getter;' +
        'import com.credits.scapi.v0.SmartContract;' +
        'import com.google.gson.*;' +

        'public class CryptoBattle extends SmartContract' +
        '{' +
        '    public String payable(BigDecimal amount, byte[] userData)' +
        '    ...' +
        '    ...' +
        '}';
            
    $.ajax({
        type: "POST",
        url: "http://descheduler.com/Api/DeploySmartContract",
        data: JSON.stringify(model),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.IsSuccess) {
                alert('Smart contract address: ' + response.Address);
            } else {
                alert('Error: ' + response.Message);
            }                    
        }
    });
