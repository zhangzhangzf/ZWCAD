
	(setvar 'cmdecho 0)		
	(setvar 'pickfirst 1)
	;(setvar 'insunits 4) ;;;����ͼ�ε�λ�Ĳ���ʱ�����ŵ�λΪ����
	(vl-load-com)
	
	
(setq sepdToolsPath "E:\\ZWCAD\\ZWCADTestProject\\bin\\Debug\\")
		

(command "NETLOAD" (strcat sepdToolsPath "ZWCADTestProject.dll"))


				
(prompt	(strcat	"\n" (getvar 'loginname)	", ��ӭʹ�ö��ο���������!"))
				
			
				
					(princ)
				