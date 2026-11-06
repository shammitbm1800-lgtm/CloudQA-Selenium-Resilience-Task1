using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Threading;

#nullable disable

class Program
{
    static int formsPassed = 0;
    static int shadowRootsFound = 0;
    static int framesFound = 0;

    static string JS_FILL_CURRENT = @"
        let root = arguments[0];
        if (!root || !root.querySelectorAll) return 0;
        let filledCount = 0;

        function setValue(el, val) {
            try {
                let desc = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value');
                if (desc && desc.set) desc.set.call(el, val);
                else el.value = val;
            } catch(e) { el.value = val; }
            el.dispatchEvent(new Event('input', {bubbles:true}));
            el.dispatchEvent(new Event('change', {bubbles:true}));
        }

        function findInput(label, tag) {
            let forAttr = label.getAttribute('for');
            if (forAttr) {
                let rootNode = label.getRootNode();
                if (rootNode && rootNode.querySelector) {
                    let el = rootNode.querySelector('#' + forAttr);
                    if (el) return el;
                }
            }
            
            let inside = label.querySelector(tag);
            if (inside) return inside;

            let curr = label.parentNode;
            for (let i = 0; i < 5 && curr; i++) {
                if (curr.querySelectorAll) {
                    let labels = Array.from(curr.querySelectorAll('label'));
                    let inputs = Array.from(curr.querySelectorAll(tag));
                    if (tag === 'input') {
                        inputs = inputs.filter(e => !['radio','checkbox','button','submit','hidden'].includes(e.type));
                    }
                    if (inputs.length > 0) {
                        let idx = labels.indexOf(label);
                        if (idx !== -1 && idx < inputs.length) return inputs[idx];
                        return inputs[0];
                    }
                }
                curr = curr.parentNode || curr.host;
            }
            return null;
        }

        function findRadio(label) {
            let curr = label.parentNode;
            for (let i = 0; i < 5 && curr; i++) {
                if (curr.querySelectorAll) {
                    let els = Array.from(curr.querySelectorAll('input[type=""radio""]'));
                    for (let r of els) {
                        if (r.value === 'Male' || r.getAttribute('aria-label') === 'Male') return r;
                    }
                }
                curr = curr.parentNode || curr.host;
            }
            return null;
        }

        let labels = root.querySelectorAll('label');
        labels.forEach(label => {
            let text = (label.textContent || '').trim();
            
            // STRICT MATCH: Prevents catching 'Billing First Name'
            if (text === 'First Name') {
                let el = findInput(label, 'input');
                if (el) { setValue(el, 'Shammit'); filledCount++; }
            }
            else if (text === 'Last Name') {
                let el = findInput(label, 'input');
                if (el && el.value !== 'Shammit') { setValue(el, 'BM'); }
            }
            else if (text.includes('Gender') || text === 'Male') {
                let el = findRadio(label);
                if (el) { 
                    el.checked = true; 
                    el.dispatchEvent(new Event('change', {bubbles:true})); 
                    el.dispatchEvent(new Event('click', {bubbles:true}));
                }
            }
            else if (text === 'State') {
                let el = findInput(label, 'select');
                if (el) {
                    for (let i = 0; i < el.options.length; i++) {
                        if (el.options[i].text.includes('India')) {
                            el.selectedIndex = i;
                            el.dispatchEvent(new Event('change', {bubbles:true}));
                            break;
                        }
                    }
                }
            }
        });

        return filledCount > 0 ? 1 : 0;
    ";

    static void ProcessAllContexts(IWebDriver driver, ISearchContext context, string path, int depth = 0)
    {
        if (depth > 20) return; 

        try
        {
            var jsContext = context is IWebDriver ? driver.FindElement(By.CssSelector("html")) : context;
            long filled = (long)((IJavaScriptExecutor)driver).ExecuteScript(JS_FILL_CURRENT, jsContext);
            if (filled > 0)
            {
                formsPassed++;
                Console.WriteLine($"PASS: {path}");
            }
        }
        catch { }

        var elements = context.FindElements(By.CssSelector("*"));
        foreach (var element in elements)
        {
            try
            {
                var shadowRoot = element.GetShadowRoot();
                if (shadowRoot != null)
                {
                    shadowRootsFound++;
                    ProcessAllContexts(driver, shadowRoot, path + " > Shadow DOM", depth + 1);
                }
            }
            catch (WebDriverException) { }
            catch (Exception) { }
        }

        var frames = context.FindElements(By.CssSelector("iframe"));
        for (int i = 0; i < frames.Count; i++)
        {
            bool switched = false;
            try
            {
                var currentFrames = context.FindElements(By.CssSelector("iframe"));
                if (i >= currentFrames.Count) continue;

                driver.SwitchTo().Frame(currentFrames[i]);
                switched = true;
                framesFound++;
                
                ProcessAllContexts(driver, driver, path + $" > IFrame [{i}]", depth + 1);
            }
            catch (Exception) { }
            finally
            {
                if (switched)
                {
                    try { driver.SwitchTo().ParentFrame(); } catch { }
                }
            }
        }
    }

    static void Main(string[] args)
    {
        string url = args.Length > 0 ? args[0] : Environment.GetEnvironmentVariable("AUTOMATION_URL") ?? "https://app.cloudqa.io/home/AutomationPracticeForm";
        using IWebDriver driver = new ChromeDriver();
        
        try
        {
            driver.Navigate().GoToUrl(url);
            driver.Manage().Window.Maximize();
            Thread.Sleep(3000); 

            Console.WriteLine("\n===== STARTING HYBRID AUTOMATION =====\n");
            
            ProcessAllContexts(driver, driver, "Main Document");

            Console.WriteLine("\n===== AUTOMATION SUMMARY =====");
            Console.WriteLine("Total Contexts Successfully Filled: " + formsPassed);
            Console.WriteLine("Shadow roots discovered: " + shadowRootsFound);
            Console.WriteLine("IFrames discovered: " + framesFound);
            Console.WriteLine("\n===== FINISHED =====");
        }
        catch (Exception ex) { Console.WriteLine("ERROR: " + ex.Message); }
        finally
        {
            Console.WriteLine("\nPress ENTER to close browser.");
            Console.ReadLine();
        }
    }
}