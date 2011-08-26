require 'fileutils'

def commit_data
  begin
    commit = `git log -1 --pretty=format:%H`
    git_date = `git log -1 --date=iso --pretty=format:%ad`
    commit_date = DateTime.parse( git_date ).strftime("%Y-%m-%d %H%M%S")
  rescue
    commit = "git unavailable"
    commit_date = Time.new.strftime("%Y-%m-%d %H%M%S")
  end
  [commit, commit_date]
end

  # copy from the key's data using the glob pattern
def nuspec_copy(key, glob)
  puts "key: #{key}, glob: #{glob}, proj dir: #{PROJECTS[key][:dir]}"
  FileList[File.join(FOLDERS[:binaries], PROJECTS[key][:dir], glob)].collect{ |f|
    to = File.join( FOLDERS[:"#{key}"][:nuspec], "lib", FRAMEWORK )
    FileUtils.mkdir_p to
    cp f, to
    # return the file name and its extension:
    File.join(FRAMEWORK, File.basename(f))
  }
end

def copy_files(from_dir, file_pattern, out_dir)
  puts "copying from #{out_dir}"
  FileUtils.mkdir_p out_dir unless FileTest.exists?(out_dir)
  Dir.glob(File.join(from_dir, file_pattern)){|file|
    copy(file, out_dir) if File.file?(file)
  }
end

def versions(str)
  str.split(/\r\n|\n/).map{|s|version(s)}.compact.sort
end

def version(str)
  ver = /v?(\d+)\.(\d+)\.(\d+)\.?(\d+)?/i.match(str).to_a()
  ver[1,4].map{|s|s.to_i} unless ver == nil or ver.empty?
end