headers<-c("COUNTER", "MSG_TYPE", "STREAM_TYPE", "DATE", "TIME", "INTERVAL", "MAC")

c<-read.csv('D:\\Back\\BigData\\SmartSpy\\[Content]', skip=0, nrows=1000000, header = FALSE, sep = " ")
d<-c[1:7]
colnames(d)<-headers
rr<-d[d[7] == "1c:bb:a8:01:50:d4"]
length(rr)
typeof(d)
#con <- file('D:\\Back\\BigData\\SmartSpy\\[Content]', "rt") 
#readLines(con, 10)
#close(con)