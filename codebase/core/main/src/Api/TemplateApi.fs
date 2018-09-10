namespace Forest.Templates.Raw

type [<Interface>] ITemplateProvider =
    abstract member Load: name:string -> Template