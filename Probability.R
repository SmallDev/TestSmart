kvant <- function(sample, min, max, n) {
  intervals <- seq(min, max, (max-min)/n)
  y <- sapply(c(1:n), function(i) {
    length(which(sample >= intervals[i] & sample < intervals[i+1])) * n / length(sample)
  })
  
  function(x) {
    i <- floor((x - min) * n / (max - min)) + 1
    i <- ifelse(i <= 0 | i > n, 0, i)
    ifelse(i == 0, 0, y[i])
  }
}

my_exp <- function(n, lambda = 1) {
  sapply(runif(count), function(x) {
    log(1-x)/(-lambda)
  })
}

#плотность может быть указана с точностью до нормировочной константы
metropolis_hastings <- function (count, dencity) {
  x <- c(1:(count+1))
  x[1] <- 0
  for(i in c(1:count)) {
    xtest <- rnorm(1, x[i])
    a <- dencity(xtest) * dnorm(x[i], xtest) / (dencity(x[i]) * dnorm(xtest, x[i]))
    x[i+1] <- ifelse(a > runif(1), xtest, x[i])
  }
  
  x[2:count]
}

xmin <- 0
xmax <- 10
count <- 100000
sample1 <- runif(count, min = xmin, max = xmax)
sample2 <- my_exp(count, lambda = 1)
plot(kvant(sample1, xmin, xmax, 10), xmin-1, xmax+1)
plot(kvant(sample2, 0, 4, 100), 0, 4)


norm <- function(x) dnorm(x, 3, 1)
sample3 <- metropolis_hastings(10000, norm)
plot(kvant(sample3, 0, 6, 50), 0, 6)

