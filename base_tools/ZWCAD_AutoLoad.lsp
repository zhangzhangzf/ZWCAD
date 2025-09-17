
	(setvar 'cmdecho 0)		
	(setvar 'pickfirst 1)
	;(setvar 'insunits 4) ;;;设置图形单位的插入时的缩放单位为毫米
	(vl-load-com)
	
	
(setq sepdToolsPath "E:\\ZWCAD\\ZWCADTestProject\\bin\\Debug\\")
		

(command "NETLOAD" (strcat sepdToolsPath "ZWCADTestProject.dll"))


				
(prompt	(strcat	"\n" (getvar 'loginname)	", 欢迎使用二次开发工具箱!"))
				
			
				
					(princ)
				